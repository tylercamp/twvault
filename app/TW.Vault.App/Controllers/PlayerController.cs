using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TW.Vault.Features.Simulation;
using TW.Vault.Features.Spatial;
using TW.Vault.Model;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Native;
using TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Player")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlayerController : BaseController
    {
        public PlayerController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet(Name = "GetPlayers")]
        public async Task<IActionResult> Get()
        {
            var players = await Paginated(CurrentSets.Player).ToListAsync();
            return Ok(players.Select(p => PlayerConvert.ModelToJson(p)));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await CurrentSets.Player.CountAsync());
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
                from village in CurrentSets.Village
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

            var ownVillageIds = await Profile("Get own village IDs", () => (
                    from player in CurrentSets.Player
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    where player.PlayerId == CurrentPlayerId
                    select village.VillageId
                ).ToListAsync()
            );
            
            var existingOutwardSupport = await Profile("Get existing outward support", () => (
                    from support in CurrentSets.CurrentVillageSupport
                                           .Include(s => s.SupportingArmy)
                    where ownVillageIds.Contains(support.SourceVillageId)
                    select support
                ).ToListAsync()
            );

            var removedSupport = (
                    from existing in existingOutwardSupport
                    where !jsonSupportData.Any((json) =>
                        json.SourceVillageId == existing.SourceVillageId &&
                        json.SupportedVillages.Any(sv => sv.Id == existing.TargetVillageId)
                    )
                    select existing
                ).ToList();

            //  Duplicates happen sometimes apparently? Just get rid of them, we'll recreate those entries with new data
            var duplicateSupport = (
                    from existing in existingOutwardSupport
                    where existingOutwardSupport.Count(cs => cs.SourceVillageId == existing.SourceVillageId && cs.TargetVillageId == existing.TargetVillageId) > 1
                    select existing
                ).ToList();

            context.CurrentVillageSupport.RemoveRange(removedSupport.Concat(duplicateSupport));

            await Profile("Save removed support", () => context.SaveChangesAsync());

            existingOutwardSupport = existingOutwardSupport.Except(removedSupport.Concat(duplicateSupport)).ToList();

            var tx = BuildTransaction(existingOutwardSupport.FirstOrDefault(s => s.TxId != null)?.TxId);
            context.Add(tx);

            Profile("Make model data", () =>
            {
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
                    scaffoldRecord = OutwardSupportConvert.ToModel(sourceVillageId, CurrentWorldId, CurrentAccessGroupId, support, scaffoldRecord, context);
                    scaffoldRecord.WorldId = CurrentWorldId;
                    scaffoldRecord.Tx = tx;
                }
            });

            await Profile("Save new support data", () => context.SaveChangesAsync());

            return Ok();
        }

        [HttpGet("stats")]
        public IActionResult GetPlayerStats()
        {
            var currentPlayerName = CurrentSets.Player.Where(p => p.PlayerId == CurrentPlayerId).Select(p => p.PlayerName).FirstOrDefault();
            if (currentPlayerName == null)
                return Ok(new JSON.UserStats());

            currentPlayerName = currentPlayerName.UrlDecode();
            var stats = Features.HighScoresService.Instance?[CurrentAccessGroupId]?.GetValueOrDefault(currentPlayerName);

            return Ok(stats ?? new JSON.UserStats());
        }

        [HttpGet("high-scores")]
        public IActionResult GetTribeHighScores() => Ok(Features.HighScoresService.Instance?[CurrentAccessGroupId] ?? new Dictionary<string, JSON.UserStats>());
    }
}
