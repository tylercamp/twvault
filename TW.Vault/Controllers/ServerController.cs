using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold;

namespace TW.Vault.Controllers
{
    //  Retrives current UTC time for synchronizing script with encryption swap interval
    //  (Don't rely on user's UTC time on their machine, TW server time can have variable
    //  offsets from UTC time depending on server)
    [Produces("application/json")]
    [Route("api/{worldName}/Server")]
    [EnableCors("AllOrigins")]
    public class ServerController : BaseController
    {
        public ServerController(VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        [HttpGet("utc")]
        public IActionResult GetCurrentUtcTime() => Ok(new { UtcTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() });

        [HttpGet("time")]
        public IActionResult GetCurrentServerTime() => Ok(new { TwTime = CurrentServerTime });

        [HttpGet("settings")]
        public IActionResult GetCurrentServerSettings() => Ok(CurrentWorldSettings);
    }
}