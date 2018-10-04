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
using Scaffold = TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;
using Newtonsoft.Json;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Player")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlayerController : BaseController
    {
        public PlayerController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }
        
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
        
        [HttpGet("{id}", Name = "GetPlayer")]
        public Task<IActionResult> Get(int id)
        {
            return SelectOr404<Scaffold.Player>((q) => q.Where(p => p.PlayerId == id).FromWorld(CurrentWorldId), PlayerConvert.ModelToJson);
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

        [HttpPost("{id}/support")]
        public async Task<IActionResult> SetOutwardsSupportData([FromBody]List<JSON.PlayerOutwardSupport> jsonSupportData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownVillageIds = await (
                    from player in context.Player.FromWorld(CurrentWorldId)
                    join village in context.Village.FromWorld(CurrentWorldId) on player.PlayerId equals village.PlayerId
                    where player.PlayerId == CurrentUser.PlayerId
                    select village.VillageId
                ).ToListAsync();
            
            var existingOutwardSupport = await (
                    from support in context.CurrentVillageSupport
                                           .FromWorld(CurrentWorldId)
                                           .Include(s => s.SupportingArmy)
                    where ownVillageIds.Contains(support.SourceVillageId)
                    select support
                ).ToListAsync();

            var removedSupport = (
                    from existing in existingOutwardSupport
                    where !jsonSupportData.Any((json) =>
                        json.SourceVillageId == existing.SourceVillageId &&
                        json.SupportedVillages.Any(sv => sv.Id == existing.TargetVillageId)
                    )
                    select existing
                ).ToList();

            context.CurrentVillageSupport.RemoveRange(removedSupport);

            foreach (var jsonData in jsonSupportData.SelectMany(jsd => jsd.SupportedVillages.Select(v => new { SourceId = jsd.SourceVillageId, Support = v })))
            {
                if (!ownVillageIds.Contains(jsonData.SourceId))
                {
                    context.Add(MakeInvalidDataRecord(JsonConvert.SerializeObject(jsonData), "Player does not own the designated source village"));
                    continue;
                }

                var sourceVillageId = jsonData.SourceId;
                var support = jsonData.Support;
                var scaffoldRecord = existingOutwardSupport.SingleOrDefault(e => e.SourceVillageId == sourceVillageId && e.TargetVillageId == support.Id);
                scaffoldRecord = OutwardSupportConvert.ToModel(sourceVillageId, CurrentWorldId, support, scaffoldRecord, context);
                scaffoldRecord.WorldId = CurrentWorldId;
            }

            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
