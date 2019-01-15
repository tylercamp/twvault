using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TW.Vault.MapDataFetcher.Controllers
{
    [Produces("application/json")]
    [Route("api/ForceRefresh")]
    [EnableCors("AllOrigins")]
    public class ForceRefreshController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get(bool? forceReApply)
        {
            return Ok((await DataFetchingService.Instance.ForceRefresh(forceReApply ?? false)) ?? (object)"No updates yet");
        }

        [HttpGet("stats")]
        public IActionResult GetLatestStats()
        {
            if (DataFetchingService.Instance.LatestStats == null)
                return Ok("No updates yet");
            else
                return Ok(DataFetchingService.Instance.LatestStats);
        }
    }
}