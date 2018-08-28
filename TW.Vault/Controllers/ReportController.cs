using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JSON = TW.Vault.Model.JSON;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Validation;
using Newtonsoft.Json;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Report")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class ReportController : BaseController
    {
        public ReportController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }


        // GET: api/Reports
        [HttpGet(Name = "GetReports")]
        public async Task<IActionResult> Get()
        {
            var reports = await Paginated(context.Report)
                    .IncludeReportData()
                    .FromWorld(CurrentWorldId)
                    .OrderByDescending(r => r.OccuredAt)
                    .ToListAsync();

            var jsonReports = reports.Select(ReportConvert.ModelToJson);

            return Ok(jsonReports);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Report.FromWorld(CurrentWorldId).CountAsync());
        }

        // GET: api/Reports/5
        [HttpGet("{reportId}", Name = "GetReport")]
        public Task<IActionResult> Get(long reportId)
        {
            return Profile("Get report by ID", () => SelectOr404<Scaffold.Report>(
                q => q.Where(r => r.ReportId == reportId).IncludeReportData().FromWorld(CurrentWorldId),
                r => ReportConvert.ModelToJson(r)
            ));
        }

        [HttpGet("village/{villageId}", Name = nameof(GetByVillage))]
        public async Task<IActionResult> GetByVillage(long villageId)
        {
            var reports = await Profile("Get reports by village", () => Paginated(
                    from report in context.Report.IncludeReportData().FromWorld(CurrentWorldId)
                    where report.DefenderVillageId == villageId || report.AttackerVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync()
            );

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }

        [HttpGet("village/{villageId}/asDefender")]
        public async Task<IActionResult> GetByDefendingVillage(long villageId)
        {
            var reports = await Profile("Get reports by defending village", () => Paginated(
                    from report in context.Report.IncludeReportData().FromWorld(CurrentWorldId)
                    where report.DefenderVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync()
            );

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }

        [HttpGet("village/{villageId}/asAttacker")]
        public async Task<IActionResult> GetByAttackingVillage(long villageId)
        {
            var reports = await Profile("Get reports by attacking village", () => Paginated(
                    from report in context.Report.IncludeReportData().FromWorld(CurrentWorldId)
                    where report.AttackerVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync()
            );

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }

        [HttpPost("check-existing-reports")]
        public async Task<IActionResult> GetExistingReports([FromBody]List<long> reportIds)
        {
            List<long> existingReports = new List<long>();

            const int maxBatchSize = 1000;
            var numBatches = (int)(Math.Ceiling(reportIds.Count / (float)maxBatchSize));
            for (int i = 0; i < numBatches; i++)
            {
                int currentBatchSize;
                if ((i + 1) * maxBatchSize > reportIds.Count)
                    currentBatchSize = reportIds.Count - i * maxBatchSize;
                else
                    currentBatchSize = maxBatchSize;

                var currentBatch = reportIds.Skip(i * maxBatchSize).Take(currentBatchSize).ToList();
                var existingBatchReports = await (
                        from report in context.Report.FromWorld(CurrentWorldId)
                        where currentBatch.Contains(report.ReportId)
                        select report.ReportId
                    ).ToListAsync();

                existingReports.AddRange(existingBatchReports);
            }

            return Ok(existingReports.ToArray());
        }
        
        // POST: api/Reports
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JSON.Report jsonReport)
        {
            if (ModelState.IsValid)
            {
                if (!Configuration.Security.ReportIgnoreExpectedPopulationBounds
                        && !ArmyValidate.MeetsPopulationRestrictions(jsonReport.AttackingArmy))
                {
                    context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                        JsonConvert.SerializeObject(jsonReport),
                        "Troops in attacking army exceed possible village population"
                    ));
                    return BadRequest();
                }

                if (!Configuration.Security.ReportIgnoreExpectedPopulationBounds
                        && !ArmyValidate.MeetsPopulationRestrictions(jsonReport.TravelingTroops))
                {
                    context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                        JsonConvert.SerializeObject(jsonReport),
                        "Troops in traveling army exceed possible village population"
                    ));
                }

                if (jsonReport.OccurredAt.Value > CurrentServerTime)
                {
                    context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                        JsonConvert.SerializeObject(jsonReport),
                        "The report 'OccurredAt' is in the future"
                    ));
                    //  Return 200/OK to trick malicious actors
                    return Ok();
                }

                var scaffoldReport = await Profile("Find existing report", () => (
                        from report in context.Report.IncludeReportData().FromWorld(CurrentWorldId)
                        where report.ReportId == jsonReport.ReportId.Value
                        select report
                    ).FirstOrDefaultAsync()
                );

                var tx = BuildTransaction();
                context.Transaction.Add(tx);

                Profile("Populate scaffold report", () =>
                {
                    if (scaffoldReport == null)
                    {
                        scaffoldReport = new Scaffold.Report();
                        scaffoldReport.WorldId = CurrentWorldId;
                        context.Report.Add(scaffoldReport);
                    }
                    else
                    {
                        var existingJsonReport = ReportConvert.ModelToJson(scaffoldReport);

                        if (existingJsonReport != jsonReport && scaffoldReport.TxId.HasValue)
                        {
                            context.ConflictingDataRecord.Add(new Scaffold.ConflictingDataRecord
                            {
                                ConflictingTx = tx,
                                OldTxId = scaffoldReport.TxId.Value
                            });
                        }
                    }

                    jsonReport.ToModel(scaffoldReport, context);

                    scaffoldReport.Tx = tx;
                });

                var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUser.Uid);
                userUploadHistory.LastUploadedReportsAt = DateTime.UtcNow;

                await Profile("Save changes", () => context.SaveChangesAsync());
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
