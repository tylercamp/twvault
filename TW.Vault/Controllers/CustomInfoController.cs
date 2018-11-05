using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

//  For random one-off data that we might log

namespace TW.Vault.Controllers
{
    public class CustomInfoData
    {
        [Required]
        public String Data { get; set; }
    }

    [Produces("application/json")]
    [Route("api/{worldName}/CustomInfo")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class CustomInfoController : BaseController
    {
        public CustomInfoController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomInfo([FromBody]CustomInfoData customInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var scaffoldInfo = new Scaffold.CustomInfo
            {
                Data = customInfo.Data,

                CreatedAt = DateTime.UtcNow,
                Uid = CurrentUserId
            };

            context.Add(scaffoldInfo);
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}