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
using Native = TW.Vault.Model.Native;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Validation;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using TW.Vault.Scaffold;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Report")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class ReportController : BaseController
    {
        public ReportController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await CurrentSets.Report.CountAsync());
        }
        
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
                    from report in CurrentSets.Report.IncludeReportData()
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
                    from report in CurrentSets.Report.IncludeReportData()
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
                    from report in CurrentSets.Report.IncludeReportData()
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
                (var existingBatchReports, var existingIgnoredBatchReports) = await ManyTasks.RunToList(
                        CurrentSets.Report.Where(r => currentBatch.Contains(r.ReportId)).Select(r => r.ReportId),
                        CurrentSets.IgnoredReport.Where(r => currentBatch.Contains(r.ReportId)).Select(r => r.ReportId)
                    );

                existingReports.AddRange(existingBatchReports);
                existingReports.AddRange(existingIgnoredBatchReports);
            }

            return Ok(existingReports.ToArray());
        }

        [HttpPost("finished-report-uploads")]
        public async Task<IActionResult> SetUserFinishedReportUploads()
        {
            var history = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
            history.LastUploadedReportsAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ignore")]
        public async Task<IActionResult> IgnoreReports([FromBody]List<long> reportIds)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            (var previousIgnoredReports, var previousReports) = await ManyTasks.RunToList(
                    CurrentSets.IgnoredReport.Where(r => reportIds.Contains(r.ReportId)).Select(r => r.ReportId),
                    CurrentSets.Report.Where(r => reportIds.Contains(r.ReportId)).Select(r => r.ReportId)
                );

            var reportsToIgnore = reportIds
                .Except(previousIgnoredReports)
                .Except(previousReports);

            foreach (var id in reportsToIgnore)
            {
                var newRecord = new Scaffold.IgnoredReport
                {
                    ReportId = id,
                    WorldId = CurrentWorldId,
                    AccessGroupId = CurrentAccessGroupId
                };

                context.Add(newRecord);
            }

            await context.SaveChangesAsync();

            var history = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
            history.LastUploadedReportsAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok();
        }

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
                        from report in CurrentSets.Report.IncludeReportData()
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
                        scaffoldReport.AccessGroupId = CurrentAccessGroupId;
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

                    jsonReport.ToModel(CurrentWorldId, scaffoldReport, context);

                    scaffoldReport.Tx = tx;
                });

                if (jsonReport.AttackingPlayerId != null)
                {
                    await Profile("Update command troop type", async () =>
                    {
                        var lostAllTroops = jsonReport.AttackingArmy == jsonReport.AttackingArmyLosses;

                        var command = await Model.UtilQuery.FindCommandForReport(scaffoldReport, context);

                        if (command == null && !lostAllTroops && (jsonReport.Loyalty == null || jsonReport.Loyalty > 0))
                        {
                            //  WARNING - This will auto-generate a command with a random ID,
                            //      if a new TW command is uploaded with the given ID any backtime
                            //      calculations for this old command will get screwy
                            try
                            {
                                await context.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }

                            command = new Scaffold.Command();
                            command.Tx = tx;
                            command.WorldId = CurrentWorldId;
                            command.AccessGroupId = CurrentAccessGroupId;
                            command.IsReturning = true;
                            command.FirstSeenAt = DateTime.UtcNow;
                            command.IsAttack = true;
                            command.SourcePlayerId = jsonReport.AttackingPlayerId.Value;
                            command.TargetPlayerId = jsonReport.DefendingPlayerId;
                            command.SourceVillageId = jsonReport.AttackingVillageId.Value;
                            command.TargetVillageId = jsonReport.DefendingVillageId.Value;
                            command.LandsAt = jsonReport.OccurredAt.Value;

                            bool madeCommand = false;

                            //  Need to auto-generate a random command ID
                            while (!madeCommand)
                            {
                                try
                                {
                                    command.CommandId = Random.NextLong >> 14;
                                    context.Add(command);
                                    await context.SaveChangesAsync();
                                    madeCommand = true;
                                }
                                catch (Exception) { }
                            }
                        }

                        JSON.TroopType? slowestType = null;
                        float slowestSpeed = -1;
                        foreach (var troopType in jsonReport.AttackingArmy.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key))
                        {
                            var travelSpeed = Native.ArmyStats.TravelSpeed[troopType];
                            if (slowestType == null)
                            {
                                slowestType = troopType;
                                slowestSpeed = travelSpeed;
                            }
                            else if (travelSpeed > slowestSpeed)
                            {
                                slowestType = troopType;
                                slowestSpeed = travelSpeed;
                            }
                        }

                        var attackingVillage = await CurrentSets.Village
                                                                .FromWorld(CurrentWorldId)
                                                                .Where(v => v.VillageId == jsonReport.AttackingVillageId)
                                                                .FirstOrDefaultAsync();

                        var defendingVillage = await CurrentSets.Village
                                                                .FromWorld(CurrentWorldId)
                                                                .Where(v => v.VillageId == jsonReport.DefendingVillageId)
                                                                .FirstOrDefaultAsync();

                        var travelCalculator = new Features.Simulation.TravelCalculator(CurrentWorldSettings.GameSpeed, CurrentWorldSettings.UnitSpeed);
                        var travelTime = travelCalculator.CalculateTravelTime(slowestType.Value, attackingVillage, defendingVillage);

                        command.TroopType = slowestType.Value.ToTroopString();

                        command.Army = ArmyConvert.JsonToArmy(jsonReport.AttackingArmy - jsonReport.AttackingArmyLosses, CurrentWorldId, command.Army, context);
                        command.Army.WorldId = CurrentWorldId;
                        command.ReturnsAt = scaffoldReport.OccuredAt + travelTime;
                        command.IsReturning = true;
                    });
                }

                //if (jsonReport.Loyalty <= 0)
                //{
                //    var conquer = new Scaffold.Conquer
                //    {
                //        WorldId = CurrentWorldId,
                //        OldOwner = jsonReport.DefendingPlayerId,
                //        NewOwner = jsonReport.AttackingPlayerId,
                //        VillageId = jsonReport.DefendingVillageId,
                //        UnixTimestamp = new DateTimeOffset(jsonReport.OccurredAt.Value).ToUnixTimeSeconds()
                //    };

                //    context.Add(conquer);
                //}

                await Profile("Save changes", () => context.SaveChangesAsync());

                //  Run upload history update in separate query to prevent creating multiple history
                //  entries
                var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
                userUploadHistory.LastUploadedReportsAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
