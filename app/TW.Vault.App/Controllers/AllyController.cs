using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold;
using TW;
using TW.Vault.Model.Convert;
using Microsoft.Extensions.DependencyInjection;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Ally")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class AllyController : BaseController
    {
        public AllyController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await Paginated(CurrentSets.Ally).ToListAsync();
            return Ok(result.Select(r => AllyConvert.ModelToJson(r)));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await CurrentSets.Ally.CountAsync());
        }
        
        [HttpGet("{id}", Name = "GetAlly")]
        public async Task<IActionResult> Get(int id)
        {
            return await SelectOr404<Ally>((q) => q.FromWorld(CurrentWorldId), (a) => AllyConvert.ModelToJson(a));
        }

        [HttpGet("{id}/members", Name = "GetTribeMembers")]
        public async Task<IActionResult> GetMembers(int id)
        {
            var players = await (
                from player in CurrentSets.Player
                where player.TribeId.Value == id
                select player
            ).ToListAsync();

            if (players.Any())
                return Ok(players.Select(p => PlayerConvert.ModelToJson(p)));
            else
                return NotFound();
        }

        [HttpGet("{id}/villages", Name = "GetTribeVillages")]
        public async Task<IActionResult> GetVillages(int id)
        {
            var villages = await Profile("GetTribeVillages", () => Paginated (
                    from player in CurrentSets.Player
                    where player.TribeId.HasValue && player.TribeId.Value == id
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId.Value
                    select village
                ).ToListAsync()
            );

            if (villages.Any())
                return Ok(villages.Select(v => VillageConvert.ModelToJson(v)));
            else
                return NotFound();
        }
    }
}
