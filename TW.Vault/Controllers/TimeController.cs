using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TW.Vault.Controllers
{
    //  Retrives current UTC time for synchronizing script with encryption swap interval
    //  (Don't rely on user's UTC time on their machine, TW server time can have variable
    //  offsets from UTC time depending on server)
    [Produces("application/json")]
    [Route("api/{worldName}/Time")]
    [EnableCors("AllOrigins")]
    public class TimeController : Controller
    {
        [HttpGet]
        public IActionResult GetCurrentUtcTime() => Ok(new { UtcTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() });
    }
}