using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        int refreshDelay = Configuration.Rankings.RefreshCheckIntervalSeconds;
        ConcurrentDictionary<int, Dictionary<String, UserStats>> cachedHighScores =
            new ConcurrentDictionary<int, Dictionary<String, UserStats>>();


        public Dictionary<String, UserStats> this[int accessGroupId]
        {
            get
            {
                if (cachedHighScores.ContainsKey(accessGroupId))
                    return cachedHighScores[accessGroupId];

                while (IsUpdating)
                    Task.Delay(100).Wait();

                return cachedHighScores.GetValueOrDefault(accessGroupId);
            }
        }

        public bool IsUpdating { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Configuration.Rankings.EnableRankingsService)
            {
                logger.LogWarning("EnableRankingsService set to false, canceling rankings service for this instance");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var accessGroups = await WithVaultContext(async (ctx) => await ctx.AccessGroup.ToListAsync()).ConfigureAwait(false);
                    IsUpdating = true;

                    var updateTasks = accessGroups.Select(async (group) =>
                    {
                        if (stoppingToken.IsCancellationRequested)
                            return;

                        Dictionary<String, UserStats> highScores = null;

                        try
                        {
                            highScores = await WithVaultContext(async (context) =>
                            {
                                return await GenerateHighScores(context, group.WorldId, group.Id, stoppingToken).ConfigureAwait(false);
                            }).ConfigureAwait(false);

                            if (highScores != null)
                                cachedHighScores[group.Id] = highScores;
                        }
                        catch (Exception e)
                        {
                            logger.LogError("Exception occurred while processing high scores for access group {0}: {1}", group.Id, e);
                        }
                    });

                    await ManyTasks.RunThrottled(updateTasks).ConfigureAwait(false);

                    IsUpdating = false;
                }
                catch (Exception e)
                {
                    logger.LogError("Exception occurred: {0}", e);
                }

                var elapsed = sw.Elapsed;
                logger.LogInformation("Updating access groups took {minutes}m {seconds}s {ms}ms", (int)elapsed.TotalMinutes, elapsed.Seconds, elapsed.Milliseconds);

                await Task.Delay(refreshDelay * 1000, stoppingToken);
            }
        }

        class SlimReport
        {
            public Scaffold.ReportArmy AttackerArmy { get; set; }
            public long AttackerPlayerId { get; set; }
            public long ReportId { get; set; }
            public long DefenderVillageId { get; set; }
            public DateTime OccuredAt { get; set; }
        }

        class SlimSupportCommand
        {
            public long TargetVillageId { get; set; }
            public long TargetPlayerId { get; set; }
            public long SourcePlayerId { get; set; }
            public DateTime LandsAt { get; set; }
        }

        public async Task<Dictionary<String, UserStats>> GenerateHighScores(Scaffold.VaultContext context, int worldId, int accessGroupId, CancellationToken ct)
        {
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            logger.LogDebug("Generating high scores for world {0}", worldId);

            var CurrentSets = new
            {
                ActiveUser = context.User.Active().FromWorld(worldId).FromAccessGroup(accessGroupId),
                Player = context.Player.FromWorld(worldId),
                Village = context.Village.FromWorld(worldId),
                CurrentVillage = context.CurrentVillage.FromWorld(worldId).FromAccessGroup(accessGroupId),
                Ally = context.Ally.FromWorld(worldId),
                CurrentVillageSupport = context.CurrentVillageSupport.FromWorld(worldId).FromAccessGroup(accessGroupId),
                Command = context.Command.FromWorld(worldId).FromAccessGroup(accessGroupId),
                Report = context.Report.FromWorld(worldId).FromAccessGroup(accessGroupId),
                EnemyTribe = context.EnemyTribe.FromWorld(worldId).FromAccessGroup(accessGroupId)
            };

            var serverSettings = await context.WorldSettings.Where(s => s.WorldId == worldId).FirstOrDefaultAsync();

            var lastWeek = serverSettings.ServerTime - TimeSpan.FromDays(7);

            logger.LogDebug("Running data queries...");

            (var tribePlayers, var tribeVillas, var tribeSupport, var tribeAttackCommands, var tribeSupportCommands, var tribeAttackingReports, var tribeDefendingReports, var enemyVillas) = await ManyTasks.Run(
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
                    join support in CurrentSets.CurrentVillageSupport
                        on village.VillageId equals support.SourceVillageId
                    select new { player.PlayerId, support.TargetVillageId, support.SupportingArmy }
                ).ToListAsync(ct)

                ,

                // Commands in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join command in CurrentSets.Command on player.PlayerId equals command.SourcePlayerId
                    where command.ArmyId != null
                    where command.LandsAt > lastWeek
                    where command.IsAttack
                    where command.TargetPlayerId != null
                    select new { command.CommandId, command.SourcePlayerId, command.LandsAt, command.TargetVillageId, command.Army }
                ).ToListAsync(ct)

                ,

                // Support commands in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join command in CurrentSets.Command on player.PlayerId equals command.SourcePlayerId
                    where command.ArmyId != null
                    where command.LandsAt > lastWeek
                    where !command.IsAttack
                    where command.TargetPlayerId != null
                    select new SlimSupportCommand { SourcePlayerId = command.SourcePlayerId, TargetPlayerId = command.TargetPlayerId.Value, TargetVillageId = command.TargetVillageId, LandsAt = command.LandsAt }
                ).ToListAsync(ct)

                ,

                // Attacking reports in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join report in CurrentSets.Report on player.PlayerId equals report.AttackerPlayerId
                    where report.OccuredAt > lastWeek
                    where report.AttackerArmy != null
                    where report.DefenderPlayerId != null
                    select new SlimReport { AttackerArmy = report.AttackerArmy, ReportId = report.ReportId, OccuredAt = report.OccuredAt, AttackerPlayerId = report.AttackerPlayerId.Value, DefenderVillageId = report.DefenderVillageId }
                ).ToListAsync(ct)

                ,

                // Defending reports in last week
                (
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join report in CurrentSets.Report on player.PlayerId equals report.DefenderPlayerId
                    where report.OccuredAt > lastWeek
                    where report.AttackerArmy != null
                    select new SlimReport { AttackerArmy = report.AttackerArmy, DefenderVillageId = report.DefenderVillageId, ReportId = report.ReportId, OccuredAt = report.OccuredAt }
                ).ToListAsync(ct)

                ,

                // Enemy villas (to determine what's "backline")
                (
                    from enemy in CurrentSets.EnemyTribe
                    join player in CurrentSets.Player on enemy.EnemyTribeId equals player.TribeId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    select new { X = village.X.Value, Y = village.Y.Value }
                ).ToListAsync(ct)
            ).ConfigureAwait(false);

            if (ct.IsCancellationRequested)
                return null;

            logger.LogDebug("Finished data queries");

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

            logger.LogDebug("Finished supplemental queries");

            var defenseReportsWithNobles = tribeDefendingReports.Where(r => r.AttackerArmy.Snob > 0).OrderBy(r => r.OccuredAt).ToList();
            var defenseNobleReportsByTargetVillage = defenseReportsWithNobles.GroupBy(r => r.DefenderVillageId).ToDictionary(g => g.Key, g => g.ToList());
            var possibleSnipesByTargetVillage = tribeSupportCommands
                .Where(c => defenseNobleReportsByTargetVillage.Keys.Contains(c.TargetVillageId))
                .GroupBy(c => c.TargetVillageId, c => c)
                .ToDictionary(g => g.Key, g => g.ToList());

            var numSnipesByPlayer = tribePlayers.ToDictionary(p => p.PlayerId, p => 0);

            foreach ((var villageId, var possibleSnipes) in possibleSnipesByTargetVillage.Tupled())
            {
                var attacksToVillage = defenseNobleReportsByTargetVillage[villageId];

                foreach (var snipe in possibleSnipes)
                {
                    var earlierReport = attacksToVillage.LastOrDefault(r => r.OccuredAt <= snipe.LandsAt);
                    var laterReport = attacksToVillage.FirstOrDefault(r => r.OccuredAt > snipe.LandsAt);

                    if (laterReport == null)
                        continue;

                    if (earlierReport != null)
                    {
                        //  Check if between two nobles that landed at around the same time
                        if (laterReport.OccuredAt - earlierReport.OccuredAt < TimeSpan.FromMilliseconds(500))
                            numSnipesByPlayer[snipe.SourcePlayerId]++;
                    }
                    else if (laterReport.OccuredAt - snipe.LandsAt < TimeSpan.FromMilliseconds(1000))
                    {
                        // Landed before 
                        numSnipesByPlayer[snipe.SourcePlayerId]++;
                    }
                }
            }



            var supportByTargetTribe = tribePlayers.ToDictionary(p => p.PlayerId, p => supportedTribeIds.ToDictionary(t => t, t => new List<Scaffold.CurrentArmy>()));
            foreach (var support in tribeSupport.Where(s => villageTribeIds.ContainsKey(s.TargetVillageId) && !tribeVillageIds.Contains(s.TargetVillageId)))
                supportByTargetTribe[support.PlayerId][villageTribeIds[support.TargetVillageId]].Add(support.SupportingArmy);

            logger.LogDebug("Sorted support by tribe");

            
            var reportsBySourcePlayer = tribePlayers.ToDictionary(p => p.PlayerId, _ => new Dictionary<long, List<SlimReport>>());
            foreach (var report in tribeAttackingReports)
            {
                var playerReports = reportsBySourcePlayer[report.AttackerPlayerId];
                if (!playerReports.ContainsKey(report.OccuredAt.Ticks))
                    playerReports.Add(report.OccuredAt.Ticks, new List<SlimReport>());
                playerReports[report.OccuredAt.Ticks].Add(report);
            }

            logger.LogDebug("Sorted reports by source player");

            var commandArmiesWithReports = new Dictionary<Scaffold.CommandArmy, SlimReport>();

            foreach (var command in tribeAttackCommands.Where(cmd => reportsBySourcePlayer.ContainsKey(cmd.SourcePlayerId)))
            {
                var matchingReport = reportsBySourcePlayer[command.SourcePlayerId].GetValueOrDefault(command.LandsAt.Ticks)?.FirstOrDefault(c => c.DefenderVillageId == command.TargetVillageId);
                if (matchingReport != null)
                    commandArmiesWithReports.Add(command.Army, matchingReport);
            }

            logger.LogDebug("Gathered commands with associated reports");

            var reportsWithoutCommands = tribeAttackingReports.Except(commandArmiesWithReports.Values).ToList();

            var usedAttackArmies = tribeAttackCommands
                .Select(c => new { c.SourcePlayerId, Army = (Army)c.Army })
                .Concat(reportsWithoutCommands.Select(r => new { SourcePlayerId = r.AttackerPlayerId, Army = (Army)r.AttackerArmy }))
                .ToList();

            logger.LogDebug("Gathered used attack armies");

            var usedAttackArmiesByPlayer = tribePlayers.ToDictionary(p => p.PlayerId, p => new List<Army>());
            foreach (var army in usedAttackArmies)
                usedAttackArmiesByPlayer[army.SourcePlayerId].Add(army.Army);

            logger.LogDebug("Sorted attack armies by player");

            var villagesByPlayer = tribeVillas.GroupBy(v => v.PlayerId).ToDictionary(g => g.Key, g => g.ToList());

            var armiesNearEnemy = new HashSet<long>();
            var enemyMap = new Spatial.Quadtree(enemyVillas.Select(v => new Coordinate { X = v.X, Y = v.Y }));
            foreach (var village in tribeVillas.Where(v => enemyMap.ContainsInRange(new Coordinate { X = v.X, Y = v.Y }, 10)))
                armiesNearEnemy.Add(village.ArmyAtHome.ArmyId);

            logger.LogDebug("Collected armies near enemies");

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
                    if (ArmyStats.IsFake(army))
                        numFakes++;
                    if (ArmyStats.IsNuke(army))
                        numNukes++;
                    if (ArmyStats.IsFang(army))
                        numFangs++;
                }

                var playerResult = new UserStats
                {
                    FangsInPastWeek = numFangs,
                    NukesInPastWeek = numNukes,
                    FakesInPastWeek = numFakes,
                    SnipesInPastWeek = numSnipesByPlayer[player.PlayerId],
                    BacklineDVsAtHome = playerVillages?.Where(v => !armiesNearEnemy.Contains(v.ArmyAtHome.ArmyId)).Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    DVsAtHome = playerVillages?.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyAtHome) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    DVsTraveling = playerVillages?.Sum(v => BattleSimulator.TotalDefensePower(v.ArmyTraveling) / (float)ArmyStats.FullDVDefensivePower) ?? 0,
                    PopPerTribe = supportByTargetTribe[player.PlayerId].Where(kvp => kvp.Value.Count > 0).ToDictionary(
                        kvp => tribeInfo[kvp.Key].Tag.UrlDecode(),
                        kvp => kvp.Value.Sum(a => BattleSimulator.TotalDefensePower(a) / (float)ArmyStats.FullDVDefensivePower)
                    )
                };

                result.Add(player.PlayerName.UrlDecode(), playerResult);
            }

            logger.LogDebug("Generated result data");

            return result;
        }
    }
}
