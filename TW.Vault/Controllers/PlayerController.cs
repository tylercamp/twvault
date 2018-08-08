using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold_Model;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Player")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlayerController : ControllerBase
    {
        public PlayerController(VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        // GET: api/Player
        [HttpGet(Name = "GetPlayers")]
        public async Task<IActionResult> Get()
        {
            return Ok(await Paginated(context.Player).ToListAsync());
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Player.CountAsync());
        }

        // GET: api/Player/5
        [HttpGet("{id}", Name = "GetPlayer")]
        public Task<IActionResult> Get(int id)
        {
            return FindOr404<Player>(id);
        }

        [HttpGet("{id}/villages")]
        public async Task<IActionResult> GetVillages(int id)
        {
            var villages = await Paginated(
                from village in context.Village
                where village.PlayerId.Value == id
                select village
            ).ToListAsync();

            if (villages.Any())
                return Ok(villages);
            else
                return NotFound();
        }
    }
}
