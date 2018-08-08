using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scaffold = TW.Vault.Scaffold_Model;
using JSON = TW.Vault.Model.JSON;
using TW.Vault.Model.Convert;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Report")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class ReportController : ControllerBase
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
                    .OrderByDescending(r => r.OccuredAt)
                    .ToListAsync();

            var jsonReports = reports.Select(ReportConvert.ModelToJson);

            return Ok(jsonReports);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Report.CountAsync());
        }

        // GET: api/Reports/5
        [HttpGet("{reportId}", Name = "GetReport")]
        public Task<IActionResult> Get(long reportId)
        {
            return FindOr404<Scaffold.Report>(
                q => q.Where(r => r.ReportId == reportId).IncludeReportData(),
                r => ReportConvert.ModelToJson(r)
            );
        }

        [HttpGet("village/{villageId}", Name = nameof(GetByVillage))]
        public async Task<IActionResult> GetByVillage(long villageId)
        {
            var reports = await Paginated(
                    from report in context.Report.IncludeReportData()
                    where report.DefenderVillageId == villageId || report.AttackerVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync();

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }

        [HttpGet("village/{villageId}/asDefender")]
        public async Task<IActionResult> GetByDefendingVillage(long villageId)
        {
            var reports = await Paginated(
                    from report in context.Report.IncludeReportData()
                    where report.DefenderVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync();

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }

        [HttpGet("village/{villageId}/asAttacker")]
        public async Task<IActionResult> GetByAttackingVillage(long villageId)
        {
            var reports = await Paginated(
                    from report in context.Report.IncludeReportData()
                    where report.AttackerVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).ToListAsync();

            var jsonReports = reports.Select(ReportConvert.ModelToJson);
            return Ok(jsonReports);
        }
        
        // POST: api/Reports
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JSON.Report jsonReport)
        {
            if (ModelState.IsValid)
            {
                var scaffoldReport = await (
                        from report in context.Report.IncludeReportData()
                        where report.ReportId == jsonReport.ReportId.Value
                        select report
                    ).FirstOrDefaultAsync();

                if (scaffoldReport == null)
                {
                    scaffoldReport = new Scaffold.Report();
                    await context.Report.AddAsync(scaffoldReport);
                }

                jsonReport.ToModel(scaffoldReport, context);

                var tx = BuildTransaction();
                await context.Transaction.AddAsync(tx);

                scaffoldReport.Tx = tx;

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
