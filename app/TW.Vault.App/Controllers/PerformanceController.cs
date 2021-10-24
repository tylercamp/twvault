using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TW.Vault.App.Controllers
{
    [Produces("application/json")]
    [Route("api/Performance")]
    [EnableCors("AllOrigins")]
    public class PerformanceController : Controller
    {
        Lib.Scaffold.VaultContext context;
        ILogger logger;

        public PerformanceController(Lib.Scaffold.VaultContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.logger = loggerFactory.CreateLogger<PerformanceController>();
        }

        [HttpGet("flush")]
        public async Task<IActionResult> FlushRecords()
        {
            logger.LogInformation("Force-flushing performance logs for IP {0}", HttpContext.Connection.RemoteIpAddress.ToString());

            var numRecords = Lib.Features.Profiling.NumPendingRecords;
            await Lib.Features.Profiling.StoreData(context, force: true, rollover: false);

            return Ok(numRecords);
        }
    }
}