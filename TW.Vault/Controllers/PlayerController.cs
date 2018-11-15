using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using JSON = TW.Vault.Model.JSON;

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

            var ownVillageIds = await Profile("Get own village IDs", () => (
                    from player in context.Player.FromWorld(CurrentWorldId)
                    join village in context.Village.FromWorld(CurrentWorldId) on player.PlayerId equals village.PlayerId
                    where player.PlayerId == CurrentPlayerId
                    select village.VillageId
                ).ToListAsync()
            );
            
            var existingOutwardSupport = await Profile("Get existing outward support", () => (
                    from support in context.CurrentVillageSupport
                                           .FromWorld(CurrentWorldId)
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
                    scaffoldRecord = OutwardSupportConvert.ToModel(sourceVillageId, CurrentWorldId, support, scaffoldRecord, context);
                    scaffoldRecord.WorldId = CurrentWorldId;
                    scaffoldRecord.Tx = tx;
                }
            });

            await Profile("Save new support data", () => context.SaveChangesAsync());

            return Ok();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPlayerStats()
        {
            LoadWorldData();

            var lastWeek = CurrentServerTime - TimeSpan.FromDays(7);

            (var ownVillas, var ownSupport, var ownCommands, var ownReports, var enemyVillas) = await ManyTasks.RunToList(

                // At-home armies
                from village in context.Village.FromWorld(CurrentWorldId)
                join currentVillage in context.CurrentVillage.FromWorld(CurrentWorldId)
                                                             .Include(cv => cv.ArmyAtHome)
                                                             .Include(cv => cv.ArmyTraveling)
                    on village.VillageId equals currentVillage.VillageId
                where village.PlayerId == CurrentPlayerId
                select new { X = village.X.Value, Y = village.Y.Value, village.VillageId, currentVillage.ArmyAtHome, currentVillage.ArmyTraveling }

                ,

                // Support
                from village in context.Village.FromWorld(CurrentWorldId)
                join support in context.CurrentVillageSupport.FromWorld(CurrentWorldId)
                                                             .Include(s => s.SupportingArmy)
                    on village.VillageId equals support.SourceVillageId
                where village.PlayerId == CurrentPlayerId
                select new { support.TargetVillageId, support.SupportingArmy }

                ,

                // Commands in last week
                from command in context.Command.FromWorld(CurrentWorldId).Include(c => c.Army)
                where command.SourcePlayerId == CurrentPlayerId
                where command.ArmyId != null
                where command.LandsAt > lastWeek
                where command.IsAttack
                select command

                ,

                // Reports in last week
                from report in context.Report.FromWorld(CurrentWorldId).Include(r => r.AttackerArmy)
                where report.AttackerPlayerId == CurrentPlayerId
                where report.OccuredAt > lastWeek
                where report.AttackerArmy != null
                select report

                ,

                // Enemy villas (to determine what's "backline")
                from enemy in context.EnemyTribe.FromWorld(CurrentWorldId)
                join player in context.Player.FromWorld(CurrentWorldId) on enemy.EnemyTribeId equals player.TribeId
                join village in context.Village.FromWorld(CurrentWorldId) on player.PlayerId equals village.PlayerId
                select new { X = village.X.Value, Y = village.Y.Value }

            );

            var ownVillageIds = ownVillas.Select(v => v.VillageId).ToList();

            var supportedVillageIds = ownSupport.Select(s => s.TargetVillageId).Distinct().ToList();
            var villageTribeIds = await (
                    from village in context.Village.FromWorld(CurrentWorldId)
                    join player in context.Player.FromWorld(CurrentWorldId) on village.PlayerId equals player.PlayerId
                    join tribe in context.Ally.FromWorld(CurrentWorldId) on player.TribeId equals tribe.TribeId
                    where supportedVillageIds.Contains(village.VillageId)
                    select new { village.VillageId, tribe.TribeId }
                ).ToDictionaryAsync(d => d.VillageId, d => d.TribeId);

            var supportedTribeIds = villageTribeIds.Values.Distinct().ToList();
            var tribeInfo = await (
                    from tribe in context.Ally.FromWorld(CurrentWorldId)
                    where supportedTribeIds.Contains(tribe.TribeId)
                    select new { tribe.TribeId, tribe.TribeName, tribe.Tag }
                ).ToDictionaryAsync(d => d.TribeId, d => new { Name = d.TribeName, d.Tag });

            var supportByTargetTribe = supportedTribeIds.ToDictionary(tid => tid, tid => new List<Scaffold.CurrentArmy>());
            foreach (var support in ownSupport.Where(s => villageTribeIds.ContainsKey(s.TargetVillageId) && !ownVillageIds.Contains(s.TargetVillageId)))
                supportByTargetTribe[villageTribeIds[support.TargetVillageId]].Add(support.SupportingArmy);


            var reportsBySourceVillage = ownReports.GroupBy(r => r.AttackerVillageId).ToDictionary(g => g.Key, g => g.ToList());
            var commandsWithReports = new Dictionary<Scaffold.Command, Scaffold.Report>();

            foreach (var command in ownCommands.Where(cmd => reportsBySourceVillage.ContainsKey(cmd.SourceVillageId)))
            {
                var matchingReport = reportsBySourceVillage[command.SourceVillageId].Where(r => r.OccuredAt == command.LandsAt && r.DefenderVillageId == command.TargetVillageId).FirstOrDefault();
                if (matchingReport != null)
                    commandsWithReports.Add(command, matchingReport);
            }

            var reportsWithoutCommands = ownReports.Except(commandsWithReports.Values).ToList();

            var usedAttackArmies = ownCommands
                .Select(c => (JSON.Army)c.Army)
                .Concat(reportsWithoutCommands.Select(r => (JSON.Army)r.AttackerArmy))
                .ToList();

            var armiesNearEnemy = new HashSet<long>();
            foreach (var village in ownVillas)
            {
                var nearbyEnemyVilla = enemyVillas.FirstOrDefault(v => Coordinate.Distance(village.X, village.Y, v.X, v.Y) < 10);
                if (nearbyEnemyVilla != null)
                    armiesNearEnemy.Add(village.ArmyAtHome.ArmyId);
            }

            var result = new JSON.UserStats
            {
                FangsInPastWeek = usedAttackArmies.Count(ArmyStats.IsFang),
                NukesInPastWeek = usedAttackArmies.Count(ArmyStats.IsNuke),
                FakesInPastWeek = usedAttackArmies.Count(ArmyStats.IsFake),
                BacklineDVsAtHome = ownVillas.Where(v => !armiesNearEnemy.Contains(v.ArmyAtHome.ArmyId)).Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower),
                DVsAtHome = ownVillas.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower),
                DVsTraveling = ownVillas.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyTraveling) / (float)ArmyStats.FullDVDefensivePower),
                PopPerTribe = supportByTargetTribe.Where(kvp => kvp.Value.Count > 0).ToDictionary(
                    kvp => WebUtility.UrlDecode(tribeInfo[kvp.Key].Tag),
                    kvp => kvp.Value.Sum(a => BattleSimulator.TotalDefensePower(a) / (float)ArmyStats.FullDVDefensivePower)
                )
            };

            return Ok(result);
        }

        [HttpGet("high-scores")]
        public IActionResult GetTribeHighScores() => Ok(Features.HighScoresService.Instance[CurrentWorldId]);
    }
}
