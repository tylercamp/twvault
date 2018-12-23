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
        public IActionResult Get()
        {
            return Ok();
        }
    }
}