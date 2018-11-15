using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.JSON;
using TW.Vault.Model.Native;

namespace TW.Vault.Features
{
    public class HighScoresService : BackgroundService
    {
        public HighScoresService(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(scopeFactory, loggerFactory)
        {
            Instance = this;
        }

        public static HighScoresService Instance { get; private set; }

        int refreshDelay = 15;
        ConcurrentDictionary<int, Dictionary<String, UserStats>> cachedHighScores =
            new ConcurrentDictionary<int, Dictionary<String, UserStats>>();


        public Dictionary<String, UserStats> this[int worldId]
        {
            get
            {
                if (cachedHighScores.ContainsKey(worldId))
                    return cachedHighScores[worldId];

                while (IsUpdating)
                    Task.Delay(100).Wait();

                return cachedHighScores.GetValueOrDefault(worldId);
            }
        }

        public bool IsUpdating { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WithVaultContext(async (context) =>
                    {
                        IsUpdating = true;

                        var worldIds = await context.World.Select(w => w.Id).ToListAsync();
                        foreach (var id in worldIds)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            var highScores = await GenerateHighScores(context, id, stoppingToken);
                            if (highScores != null)
                                cachedHighScores[id] = highScores;
                        }

                        IsUpdating = false;
                    });
                }
                catch (Exception e)
                {
                    logger.LogError("Exception occurred: {0}", e);
                }

                await Task.Delay(refreshDelay * 1000, stoppingToken);
            }
        }

        private async Task<Dictionary<String, UserStats>> GenerateHighScores(Scaffold.VaultContext context, int worldId, CancellationToken ct)
        {
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var CurrentSets = new
            {
                ActiveUser = context.User.Active().FromWorld(worldId),
                Player = context.Player.FromWorld(worldId),
                Village = context.Village.FromWorld(worldId),
                CurrentVillage = context.CurrentVillage.FromWorld(worldId),
                Ally = context.Ally.FromWorld(worldId),
                CurrentVillageSupport = context.CurrentVillageSupport.FromWorld(worldId),
                Command = context.Command.FromWorld(worldId),
                Report = context.Report.FromWorld(worldId),
                EnemyTribe = context.EnemyTribe.FromWorld(worldId)
            };

            var serverTimeOffset = await context.WorldSettings.Where(s => s.WorldId == worldId).Select(s => s.UtcOffset).FirstOrDefaultAsync();

            var lastWeek = DateTime.UtcNow + serverTimeOffset - TimeSpan.FromDays(7);

            (var tribePlayers, var tribeVillas, var tribeSupport, var tribeCommands, var tribeReports, var enemyVillas) = await ManyTasks.Run(
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    select new { player.PlayerName, player.PlayerId }
                ).ToListAsync(ct)

                ,

                // At-home armies
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    join currentVillage in CurrentSets.CurrentVillage
                                                      .Include(cv => cv.ArmyAtHome)
                                                      .Include(cv => cv.ArmyTraveling)
                        on village.VillageId equals currentVillage.VillageId
                    where currentVillage.ArmyAtHomeId != null && currentVillage.ArmyTravelingId != null
                    select new { X = village.X.Value, Y = village.Y.Value, player.PlayerId, village.VillageId, currentVillage.ArmyAtHome, currentVillage.ArmyTraveling }
                ).ToListAsync(ct)

                ,

                // Support
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    join support in CurrentSets.CurrentVillageSupport.Include(s => s.SupportingArmy)
                        on village.VillageId equals support.SourceVillageId
                    select new { player.PlayerId, support.TargetVillageId, support.SupportingArmy }
                ).ToListAsync(ct)

                ,

                // Commands in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join command in CurrentSets.Command.Include(c => c.Army) on player.PlayerId equals command.SourcePlayerId
                    where command.ArmyId != null
                    where command.LandsAt > lastWeek
                    where command.IsAttack
                    select command
                ).ToListAsync(ct)

                ,

                // Reports in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join report in CurrentSets.Report.Include(r => r.AttackerArmy) on player.PlayerId equals report.AttackerPlayerId
                    where report.OccuredAt > lastWeek
                    where report.AttackerArmy != null
                    select report
                ).ToListAsync(ct)

                ,

                // Enemy villas (to determine what's "backline")
                (
                    from enemy in CurrentSets.EnemyTribe
                    join player in CurrentSets.Player on enemy.EnemyTribeId equals player.TribeId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    select new { X = village.X.Value, Y = village.Y.Value }
                ).ToListAsync(ct)
            );

            if (ct.IsCancellationRequested)
                return null;

            // SLIM COMMANDS
            //var slimTribeCommands = await (
            //    from user in CurrentSets.ActiveUser
            //    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
            //    join command in CurrentSets.Command.Include(c => c.Army) on player.PlayerId equals command.SourcePlayerId
            //    where command.ArmyId != null
            //    where command.LandsAt > lastWeek
            //    where command.IsAttack
            //    select new { command.CommandId, command.SourceVillageId, command.TargetVillageId, command.Army }
            //).ToListAsync();

            // SLIM
            //var slimTribeReports = await (
            //    from user in CurrentSets.ActiveUser
            //    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
            //    join report in CurrentSets.Report.Include(r => r.AttackerArmy) on player.PlayerId equals report.AttackerPlayerId
            //    where report.OccuredAt > lastWeek
            //    where report.AttackerArmy != null
            //    select new { report.AttackerArmy, report.ReportId, report.OccuredAt, report.AttackerVillageId }
            //).ToListAsync();

            var tribeVillageIds = tribeVillas.Select(v => v.VillageId).ToList();

            var supportedVillageIds = tribeSupport.Select(s => s.TargetVillageId).Distinct().ToList();
            var villageTribeIds = await (
                from village in CurrentSets.Village
                join player in CurrentSets.Player on village.PlayerId equals player.PlayerId
                join tribe in CurrentSets.Ally on player.TribeId equals tribe.TribeId
                where supportedVillageIds.Contains(village.VillageId)
                select new { village.VillageId, tribe.TribeId }
            ).ToDictionaryAsync(d => d.VillageId, d => d.TribeId, ct);

            if (ct.IsCancellationRequested)
                return null;

            var supportedTribeIds = villageTribeIds.Values.Distinct().ToList();
            var tribeInfo = await (
                from tribe in CurrentSets.Ally
                where supportedTribeIds.Contains(tribe.TribeId)
                select new { tribe.TribeId, tribe.TribeName, tribe.Tag }
            ).ToDictionaryAsync(d => d.TribeId, d => new { Name = d.TribeName, d.Tag }, ct);

            if (ct.IsCancellationRequested)
                return null;

            var supportByTargetTribe = tribePlayers.ToDictionary(p => p.PlayerId, p => supportedTribeIds.ToDictionary(t => t, t => new List<Scaffold.CurrentArmy>()));
            foreach (var support in tribeSupport.Where(s => villageTribeIds.ContainsKey(s.TargetVillageId) && !tribeVillageIds.Contains(s.TargetVillageId)))
                supportByTargetTribe[support.PlayerId][villageTribeIds[support.TargetVillageId]].Add(support.SupportingArmy);


            var reportsBySourceVillage = tribeReports.GroupBy(r => r.AttackerVillageId).ToDictionary(g => g.Key, g => g.ToList());
            var commandsWithReports = new Dictionary<Scaffold.Command, Scaffold.Report>();

            foreach (var command in tribeCommands.Where(cmd => reportsBySourceVillage.ContainsKey(cmd.SourceVillageId)))
            {
                var matchingReport = reportsBySourceVillage[command.SourceVillageId].Where(r => r.OccuredAt == command.LandsAt && r.DefenderVillageId == command.TargetVillageId).FirstOrDefault();
                if (matchingReport != null)
                    commandsWithReports.Add(command, matchingReport);
            }

            var reportsWithoutCommands = tribeReports.Except(commandsWithReports.Values).ToList();

            var usedAttackArmies = tribeCommands
                .Select(c => new { c.SourcePlayerId, Army = (Army)c.Army })
                .Concat(reportsWithoutCommands.Select(r => new { SourcePlayerId = r.AttackerPlayerId.Value, Army = (Army)r.AttackerArmy }))
                .ToList();

            var usedAttackArmiesByPlayer = tribePlayers.ToDictionary(p => p.PlayerId, p => new List<Army>());
            foreach (var army in usedAttackArmies)
                usedAttackArmiesByPlayer[army.SourcePlayerId].Add(army.Army);

            var villagesByPlayer = tribeVillas.GroupBy(v => v.PlayerId).ToDictionary(g => g.Key, g => g.ToList());

            var armiesNearEnemy = new HashSet<long>();
            var enemyMap = new Spatial.Quadtree(enemyVillas.Select(v => new Coordinate { X = v.X, Y = v.Y }));
            foreach (var village in tribeVillas.Where(v => enemyMap.ContainsInRange(new Coordinate { X = v.X, Y = v.Y }, 10)))
                armiesNearEnemy.Add(village.ArmyAtHome.ArmyId);

            var result = new Dictionary<String, UserStats>();
            foreach (var player in tribePlayers)
            {
                if (ct.IsCancellationRequested)
                    break;

                var playerVillages = villagesByPlayer.GetValueOrDefault(player.PlayerId);
                var playerArmies = usedAttackArmiesByPlayer[player.PlayerId];

                int numFangs = 0, numNukes = 0, numFakes = 0;
                foreach (var army in playerArmies)
                {
                    if (ArmyStats.IsFake(army)) numFakes++;
                    if (ArmyStats.IsNuke(army)) numNukes++;
                    if (ArmyStats.IsFang(army)) numFangs++;
                }

                var playerResult = new UserStats
                {
                    FangsInPastWeek = numFangs,
                    NukesInPastWeek = numNukes,
                    FakesInPastWeek = numFakes,
                    BacklineDVsAtHome = playerVillages?.Where(v => !armiesNearEnemy.Contains(v.ArmyAtHome.ArmyId)).Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    DVsAtHome = playerVillages?.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    DVsTraveling = playerVillages?.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyTraveling) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    PopPerTribe = supportByTargetTribe[player.PlayerId].Where(kvp => kvp.Value.Count > 0).ToDictionary(
                        kvp => WebUtility.UrlDecode(tribeInfo[kvp.Key].Tag),
                        kvp => kvp.Value.Sum(a => BattleSimulator.TotalDefensePower(a) / (float)ArmyStats.FullDVDefensivePower)
                    )
                };

                result.Add(WebUtility.UrlDecode(player.PlayerName), playerResult);
            }

            return result;
        }
    }
}
