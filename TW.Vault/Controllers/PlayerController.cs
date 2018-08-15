using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;
using TW.Vault.Scaffold;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Player")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlayerController : BaseController
    {
        public PlayerController(VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        // GET: api/Player
        [HttpGet(Name = "GetPlayers")]
        public async Task<IActionResult> Get()
        {
            var players = await Paginated(context.Player).FromWorld(CurrentWorldId).ToListAsync();
            return Ok(players.Select(p => PlayerConvert.ModelToJson(p)));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Player.FromWorld(CurrentWorldId).CountAsync());
        }

        // GET: api/Player/5
        [HttpGet("{id}", Name = "GetPlayer")]
        public Task<IActionResult> Get(int id)
        {
            return SelectOr404<Player>((q) => q.Where(p => p.PlayerId == id).FromWorld(CurrentWorldId), PlayerConvert.ModelToJson);
        }

        [HttpGet("{id}/villages")]
        public async Task<IActionResult> GetVillages(int id)
        {
            var villages = await Paginated(
                from village in context.Village.FromWorld(CurrentWorldId)
                where village.PlayerId.Value == id
                select village
            ).ToListAsync();

            if (villages.Any())
                return Ok(villages.Select(v => VillageConvert.ModelToJson(v)));
            else
                return NotFound();
        }
    }
}
