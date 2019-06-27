using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Features.Planning.Requirements;
using TW.Vault.Features.Planning.Requirements.Modifiers;
using TW.Vault.Model;
using TW.Vault.Model.Native;
using TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Alert")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class AlertController : BaseController
    {
        public AlertController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestedActions()
        {
            PreloadWorldData();
            PreloadTranslationData();

            var serverTime = CurrentServerTime;
            var twoDaysAgo = serverTime - TimeSpan.FromDays(2);
            var twoDaysAgoTimestamp = new DateTimeOffset(twoDaysAgo).ToUnixTimeSeconds();

            var ownVillageData = await ManyTasks.RunToList(
                from village in CurrentSets.Village
                join currentVillage in CurrentSets.CurrentVillage
                    on village.VillageId equals currentVillage.VillageId
                where village.PlayerId == CurrentPlayerId
                select new { X = village.X.Value, Y = village.Y.Value, village.VillageId, VillageName = village.VillageName.UrlDecode(), currentVillage.ArmyAtHome, currentVillage.ArmyStationed }
            );

            var vaultPlayerIds = await CurrentSets.ActiveUser.Select(u => u.PlayerId).ToListAsync();
            var ownVillageMap = new Features.Spatial.Quadtree(ownVillageData.Select(v => new Coordinate { X = v.X, Y = v.Y }));

            async Task<object> GetRecapSuggestions(CurrentContextDbSets CurrentSets)
            {
                var capturedVillages = await (
                    from conquer in CurrentSets.Conquer
                    join sourcePlayer in CurrentSets.ActiveUser on conquer.OldOwner equals sourcePlayer.PlayerId
                    join village in CurrentSets.Village on conquer.VillageId equals village.VillageId
                    where conquer.UnixTimestamp > twoDaysAgoTimestamp
                    where conquer.NewOwner == null || !vaultPlayerIds.Contains(conquer.NewOwner.Value)
                    where conquer.NewOwner == village.PlayerId
                    select new { X = village.X.Value, Y = village.Y.Value, VillageId = conquer.VillageId, village.VillageName, conquer.OldOwner, conquer.NewOwner, OccurredAt = DateTimeOffset.FromUnixTimeSeconds(conquer.UnixTimestamp).UtcDateTime }
                ).ToListAsync();

                capturedVillages = capturedVillages
                    .GroupBy(v => v.VillageId)
                    .Select(g => g.OrderByDescending(v => v.OccurredAt).First())
                    .ToList();

                var villageIds = capturedVillages.Select(v => v.VillageId).ToList();
                var noblesToVillages = await (
                    from user in CurrentSets.ActiveUser
                    join command in CurrentSets.Command on user.PlayerId equals command.SourcePlayerId
                    where villageIds.Contains(command.TargetVillageId)
                    where command.ArmyId != null && command.Army.Snob > 0
                    where command.LandsAt > serverTime
                    select command
                ).ToListAsync();

                var noblesToVillagesById = noblesToVillages.GroupBy(v => v.TargetVillageId).ToDictionary(g => g.Key, g => g.Count());

                var relevantPlayerIds = capturedVillages
                    .Select(v => v.OldOwner.Value)
                    .Concat(capturedVillages.Where(v => v.NewOwner != null).Select(v => v.NewOwner.Value))
                    .Distinct()
                    .ToList();

                var playerNamesById = await
                    CurrentSets.Player.Where(p => relevantPlayerIds.Contains(p.PlayerId))
                    .ToDictionaryAsync(p => p.PlayerId, p => p.PlayerName);

                var loyaltyCalculator = new Features.Simulation.LoyaltyCalculator(CurrentWorldSettings.GameSpeed);
                var possibleLoyalties = capturedVillages.ToDictionary(
                    v => v.VillageId,
                    v => loyaltyCalculator.PossibleLoyalty(25, serverTime - v.OccurredAt)
                );

                var tlNONE = await TranslateAsync("NONE");

                return capturedVillages
                    .Where(v => possibleLoyalties[v.VillageId] < 100)
                    .Where(v => noblesToVillagesById.GetValueOrDefault(v.VillageId, 0) * CurrentWorldSettings.NoblemanLoyaltyMax < possibleLoyalties[v.VillageId])
                    .Select(v => new
                    {
                        v.OccurredAt,
                        v.X, v.Y, v.VillageId,
                        VillageName = v.VillageName.UrlDecode(), 
                        OldOwnerId = v.OldOwner, NewOwnerId = v.NewOwner,
                        OldOwnerName = playerNamesById.GetValueOrDefault(v.OldOwner ?? -1, tlNONE).UrlDecode(),
                        NewOwnerName = playerNamesById.GetValueOrDefault(v.NewOwner ?? -1, tlNONE).UrlDecode(),
                        IsNearby = ownVillageMap.ContainsInRange(v.X, v.Y, 5),
                        Loyalty = possibleLoyalties[v.VillageId]
                    })
                    .OrderBy(v => v.Loyalty)
                    .ToList();
            }

            async Task<object> GetSnipeSuggestions(CurrentContextDbSets CurrentSets)
            {
                var incomingNobles = await (
                    from user in CurrentSets.ActiveUser
                    join command in CurrentSets.Command on user.PlayerId equals command.TargetPlayerId
                    where command.IsAttack
                    where command.TroopType == "snob"
                    where command.LandsAt > serverTime
                    select new { command.TargetVillageId, command.LandsAt }
                ).ToListAsync();

                var incomingTrains = incomingNobles
                    .GroupBy(n => n.TargetVillageId)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .OrderBy(n => n.LandsAt)
                            .GroupWhile((prev, curr) => prev.LandsAt - curr.LandsAt < TimeSpan.FromSeconds(1))
                            .Select(t =>
                            {
                                var train = t.ToList();
                                return new { train.First().LandsAt, Range = train.Last().LandsAt - train.First().LandsAt, Train = train };
                            })
                            .ToList()
                    );

                var targetVillageIds = incomingTrains.Keys.ToList();
                var targetVillageInfo = await (
                    from village in CurrentSets.Village
                    join currentVillage in CurrentSets.CurrentVillage.Include(cv => cv.ArmyStationed) on village.VillageId equals currentVillage.VillageId
                    where targetVillageIds.Contains(village.VillageId)
                    select new { Village = village, CurrentVillage = currentVillage }
                ).ToListAsync();

                var ownVillages = await (
                    from village in CurrentSets.Village
                    join currentVillage in CurrentSets.CurrentVillage.Include(cv => cv.ArmyAtHome) on village.VillageId equals currentVillage.VillageId
                    where village.PlayerId == CurrentPlayerId
                    where currentVillage.ArmyAtHomeId != null
                    select new { Village = village, CurrentVillage = currentVillage }
                ).ToDictionaryAsync(d => d.Village, d => d.CurrentVillage);

                var planner = new Features.Planning.CommandOptionsCalculator(CurrentWorldSettings);
                var timeRequirement = new MaximumTravelTimeRequirement();
                planner.Requirements.Add(new MinimumDefenseRequirement { MinimumDefense = 10000 }.LimitTroopType(ArmyStats.DefensiveTroopTypes));
                planner.Requirements.Add(timeRequirement);

                return targetVillageInfo
                    // Don't bother sniping stacked villas
                    .Where(info =>
                    {
                        var defPop = ArmyStats.CalculateTotalPopulation(info.CurrentVillage.ArmyStationed, ArmyStats.DefensiveTroopTypes);
                        var numDVsStationed = defPop / (float)Features.CommandClassification.Utils.FullArmyPopulation;
                        return numDVsStationed < 2;
                    })
                    // Make a plan for each train
                    .SelectMany(info => incomingTrains[info.Village.VillageId].Select(train =>
                    {
                        timeRequirement.MaximumTime = train.LandsAt - serverTime;
                        var plan = planner.GenerateOptions(ownVillages, info.Village).Take(100).ToList();
                        return new
                        {
                            info.Village,
                            Train = train,
                            Plan = plan
                        };
                    }))
                    // Ignore plans without any instructions
                    .Where(info => info.Plan.Count > 0)
                    // For each train and target village, return a plan with info on the villa that needs to be sniped
                    .Select(info => new
                    {
                        Plan = info.Plan,
                        Train = info.Train.Train,
                        LandsAt = info.Train.LandsAt,
                        TargetVillage = new {
                            Id = info.Village.VillageId,
                            Name = info.Village.VillageName.UrlDecode(),
                            X = info.Village.X.Value,
                            Y = info.Village.Y.Value
                        }
                    })
                    .OrderBy(info => info.LandsAt)
                    .ToList();
            }

            async Task<object> GetStackSuggestions(CurrentContextDbSets CurrentSets)
            {
                var enemyVillages = await ManyTasks.RunToList(
                    from enemy in CurrentSets.EnemyTribe
                    join player in CurrentSets.Player on enemy.EnemyTribeId equals player.TribeId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    select new Coordinate { X = village.X.Value, Y = village.Y.Value }
                );

                var enemyMap = new Features.Spatial.Quadtree(enemyVillages);
                var frontlineVillages = ownVillageData.Where(v => enemyMap.ContainsInRange(v.X, v.Y, 4.0f)).ToList();
                var frontlineVillageIds = frontlineVillages.Select(v => v.VillageId).ToList();

                (var attacksOnFrontline, var supportToFrontine) = await ManyTasks.RunToList(
                    from command in CurrentSets.Command
                    where frontlineVillageIds.Contains(command.TargetVillageId)
                    where command.IsAttack
                    where command.LandsAt > serverTime
                    select new { command.SourceVillageId, command.TargetVillageId }

                    ,

                    from support in CurrentSets.Command
                    where frontlineVillageIds.Contains(support.TargetVillageId)
                    where support.LandsAt > serverTime
                    where support.ArmyId != null
                    select new { support.TargetVillageId, support.Army }
                );

                var attackingVillageIds = attacksOnFrontline.Select(c => c.SourceVillageId).Distinct().ToList();
                var attackingVillages = await CurrentSets
                    .CurrentVillage
                    .Where(v => attackingVillageIds.Contains(v.VillageId))
                    .Select(v => new { v.VillageId, v.ArmyOwned, v.ArmyStationed, v.ArmyTraveling })
                    .ToDictionaryAsync(
                        v => v.VillageId,
                        v => v
                    );

                var attackingVillagesWithNukes = attackingVillages
                    // Get possible troops from village
                    .Select(kvp => new { kvp.Key, Army = JSON.Army.Max(kvp.Value.ArmyOwned, kvp.Value.ArmyStationed, kvp.Value.ArmyTraveling) })
                    // Get population of offensive troops
                    .Select(kvp => new { kvp.Key, OffensivePop = ArmyStats.CalculateTotalPopulation(kvp.Army, ArmyStats.OffensiveTroopTypes.Except(new[] { JSON.TroopType.Heavy }).ToArray()) })
                    // Filter by full nukes
                    .Where(kvp => kvp.OffensivePop > 0.65f * Features.CommandClassification.Utils.FullArmyPopulation)
                    .Select(kvp => kvp.Key)
                    .ToList();

                var nukesSentPerVillage = frontlineVillageIds.ToDictionary(id => id, id => 0);
                foreach (var attack in attacksOnFrontline.Where(a => attackingVillagesWithNukes.Contains(a.SourceVillageId)))
                    nukesSentPerVillage[attack.TargetVillageId]++;

                var pendingSupportPerVillage = frontlineVillageIds.ToDictionary(id => id, id => new JSON.Army());
                foreach (var support in supportToFrontine)
                    pendingSupportPerVillage[support.TargetVillageId] += support.Army;

                var battleSimulator = new Features.Simulation.BattleSimulator();
                var nukesEatablePerVillage = frontlineVillages.ToDictionary(
                    v => v.VillageId,
                    v => battleSimulator.EstimateRequiredNukes(v.ArmyStationed + pendingSupportPerVillage[v.VillageId], 20, CurrentWorldSettings.ArchersEnabled, 100).NukesRequired
                );

                return frontlineVillages
                    .Where(v => nukesSentPerVillage[v.VillageId] > 0 && nukesEatablePerVillage[v.VillageId] - nukesSentPerVillage[v.VillageId] < 2)
                    .Select(v => new
                    {
                        v.VillageId,
                        v.VillageName,
                        v.X, v.Y,
                        SentNukes = nukesSentPerVillage[v.VillageId],
                        EatableNukes = nukesEatablePerVillage[v.VillageId]
                    })
                    .OrderByDescending(v => v.SentNukes - v.EatableNukes)
                    .ThenBy(v => v.VillageId)
                    .ToList();
            }

            async Task<object> GetNobleTargetSuggestions(CurrentContextDbSets CurrentSets)
            {
                (var villasWithNobles, var enemyCurrentVillas) = await ManyTasks.RunToList(
                    from village in CurrentSets.Village
                    join currentVillage in CurrentSets.CurrentVillage on village.VillageId equals currentVillage.VillageId
                    where currentVillage.ArmyOwned.Snob > 0
                    where village.PlayerId == CurrentPlayerId
                    select new Coordinate { X = village.X.Value, Y = village.Y.Value }

                    ,

                    from currentVillage in CurrentSets.CurrentVillage
                    join village in CurrentSets.Village on currentVillage.VillageId equals village.VillageId
                    where !CurrentSets.ActiveUser.Any(au => au.PlayerId == village.PlayerId)
                    select new { X = village.X.Value, Y = village.Y.Value, village.VillageId, village.Points, currentVillage.Loyalty, currentVillage.LoyaltyLastUpdated, currentVillage.ArmyStationed, village.PlayerId, VillageName = village.VillageName.UrlDecode(), PlayerName = village.Player.PlayerName.UrlDecode() }
                );

                var villageMap = new Features.Spatial.Quadtree(villasWithNobles);

                var loyaltyCalculator = new Features.Simulation.LoyaltyCalculator(CurrentWorldSettings.GameSpeed);
                var possibleTargets = enemyCurrentVillas
                    .Where(v => villageMap.ContainsInRange(v.X, v.Y, 7.5f)) // Only consider enemy villas within 7.5 fields of any villa with nobles
                    .Select(v =>
                    {
                        var possibleLoyalty = v.Loyalty.HasValue
                            ? loyaltyCalculator.PossibleLoyalty(v.Loyalty.Value, serverTime - v.LoyaltyLastUpdated.Value)
                            : 100;

                        var stationedDVs = ArmyStats.CalculateTotalPopulation(v.ArmyStationed, ArmyStats.DefensiveTroopTypes) / (float)Features.CommandClassification.Utils.FullArmyPopulation;

                        // Select "confidence" in selecting the given target as a suggestion
                        // If < 0.75 DV stationed or loyalty under 50, 100% confident in the suggestion
                        var loyaltyConfidence = 1.0f - (possibleLoyalty - 50) / 50.0f;
                        var stackConfidence = 1.0f - (stationedDVs - 0.75f) / 0.75f;

                        if (v.ArmyStationed?.LastUpdated != null)
                        {
                            var stackAge = serverTime - v.ArmyStationed.LastUpdated.Value;
                            var ageFactor = (TimeSpan.FromHours(48) - stackAge) / TimeSpan.FromHours(48);
                            stackConfidence *= Math.Max(0, (float)Math.Pow(Math.Abs(ageFactor), 0.5f) * Math.Sign(ageFactor));
                        }
                        else
                        {
                            stackConfidence = 0;
                        }

                        return new
                        {
                            Loyalty = possibleLoyalty,
                            StationedDVs = stationedDVs,
                            DVsSeenAt = v.ArmyStationed?.LastUpdated,
                            Confidence = loyaltyConfidence + stackConfidence,
                            Village = v
                        };
                    });

                var confidentTargets = possibleTargets.Where(t => t.Confidence > 1).ToList();
                return confidentTargets
                    .Select(t => new
                    {
                        t.Village.X,
                        t.Village.Y,
                        t.Village.VillageId,
                        t.Village.VillageName,
                        t.Village.PlayerId,
                        t.Village.PlayerName,
                        t.Village.Points,
                        t.Loyalty,
                        t.StationedDVs,
                        t.DVsSeenAt,
                        t.Confidence
                    })
                    .OrderByDescending(t => t.Confidence)
                    .ThenBy(t => t.VillageId)
                    .ToList();
            }

            async Task<object> GetUselessStackSuggestions(CurrentContextDbSets CurrentSets)
            {
                (var enemyVillages, var ownSupport) = await ManyTasks.RunToList(
                    from enemy in CurrentSets.EnemyTribe
                    join player in CurrentSets.Player on enemy.EnemyTribeId equals player.TribeId
                    join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                    select new Coordinate { X = village.X.Value,  Y = village.Y.Value }

                    ,

                    from support in CurrentSets.CurrentVillageSupport
                    join targetVillage in CurrentSets.Village on support.TargetVillageId equals targetVillage.VillageId
                    join sourceVillage in CurrentSets.Village on support.SourceVillageId equals sourceVillage.VillageId
                    where targetVillage.PlayerId == null || !vaultPlayerIds.Contains(targetVillage.PlayerId.Value)
                    where sourceVillage.PlayerId == CurrentPlayerId
                    select new { support.SupportingArmy, targetVillage.VillageId }
                );

                var notOwnVillageIds = ownSupport.Select(s => s.VillageId).Distinct().ToList();
                var notOwnVillages = await (
                        from village in CurrentSets.Village
                        where notOwnVillageIds.Contains(village.VillageId)
                        select new { X = village.X.Value, Y = village.Y.Value, VillageName = village.VillageName.UrlDecode(), village.VillageId }
                    ).ToListAsync();

                var enemyMap = new Features.Spatial.Quadtree(enemyVillages);
                
                var backlineVillages = ownVillageData.Where(v => !enemyMap.ContainsInRange(new Coordinate { X = v.X, Y = v.Y }, 10));

                var stackInfo = backlineVillages
                    // Select backline villages with over 3k pop of support
                    .Select(v => new { Village = new { v.X, v.Y, v.VillageName, v.VillageId }, Population = ArmyStats.CalculateTotalPopulation((JSON.Army)v.ArmyStationed - (JSON.Army)v.ArmyAtHome) })
                    .Where(v => v.Population > 3000)
                    .Select(v => new { v.Village.X, v.Village.Y, v.Village.VillageName, v.Village.VillageId, PopCount = v.Population })
                    // Include support to unknown villages
                    .Concat(notOwnVillages.Select(v => new { v.X, v.Y, v.VillageName, v.VillageId, PopCount = ownSupport.Where(s => s.VillageId == v.VillageId).Sum(s => ArmyStats.CalculateTotalPopulation(s.SupportingArmy)) }))
                    .ToList();

                var villageIds = stackInfo.Select(i => i.VillageId).Distinct().ToList();
                var villageOwnersById = await (
                    from village in CurrentSets.Village
                    join player in CurrentSets.Player on village.PlayerId equals player.PlayerId
                    let tribe = (from tribe in CurrentSets.Ally
                                 where tribe.TribeId == player.TribeId
                                 select new { tribe.Tag, tribe.TribeId }).FirstOrDefault()
                    where villageIds.Contains(village.VillageId)
                    select new { village.VillageId, Player = player, Tribe = tribe }
                ).ToDictionaryAsync(
                    v => v.VillageId,
                    v => new { v.Player.PlayerName, v.Player.PlayerId, TribeName = v.Tribe?.Tag, v.Tribe?.TribeId }
                );

                return stackInfo
                    .Select(info =>
                    {
                        var owner = villageOwnersById.GetValueOrDefault(info.VillageId);
                        return new
                        {
                            info.VillageId,
                            info.VillageName,
                            info.PopCount,
                            info.X,
                            info.Y,
                            PlayerName = owner?.PlayerName == null ? null : owner.PlayerName.UrlDecode(),
                            owner?.PlayerId,
                            TribeName = owner?.TribeName == null ? null : owner.TribeName.UrlDecode(),
                            owner?.TribeId
                        };
                    });
            }

            (var recaps, var snipes, var stacks, var nobles, var uselessStacks) = await ManyTasks.Run(
                    WithTemporarySets(GetRecapSuggestions),
                    WithTemporarySets(GetSnipeSuggestions),
                    WithTemporarySets(GetStackSuggestions),
                    WithTemporarySets(GetNobleTargetSuggestions),
                    WithTemporarySets(GetUselessStackSuggestions)
                );

            return Ok(new
            {
                Recaps = recaps,
                Snipes = snipes,
                Stacks = stacks,
                NobleTargets = nobles,
                UselessStacks = uselessStacks
            });
        }

        [HttpGet("nearby-support")]
        public async Task<IActionResult> GetNearbySupportPlayers(short x, short y, int maxTravelSeconds)
        {
            var support = await (
                from user in CurrentSets.ActiveUser
                join village in CurrentSets.Village on user.PlayerId equals village.PlayerId
                join currentVillage in CurrentSets.CurrentVillage.Include(cv => cv.ArmyAtHome) on village.VillageId equals currentVillage.VillageId
                where currentVillage.ArmyAtHomeId != null
                select new { Village = village, CurrentVillage = currentVillage }
            ).ToDictionaryAsync(
                v => v.Village,
                v => v.CurrentVillage
            );

            var planner = new Features.Planning.CommandOptionsCalculator(CurrentWorldSettings);
            planner.Requirements.Add(new MinimumDefenseRequirement { MinimumDefense = 10000 }.LimitTroopType(ArmyStats.DefensiveTroopTypes));
            planner.Requirements.Add(new MaximumTravelTimeRequirement { MaximumTime = TimeSpan.FromSeconds(maxTravelSeconds) });

            var options = planner.GenerateOptions(support, new Village { X = x, Y = y });
            var villageIds = options.Select(o => o.SendFrom).Distinct().ToList();
            var playerIds = await CurrentSets.Village.Where(v => villageIds.Contains(v.VillageId)).Select(v => v.PlayerId.Value).Distinct().ToListAsync();

            if (playerIds.Contains(CurrentPlayerId))
                playerIds.Remove(CurrentPlayerId);

            var players = await CurrentSets.Player.Where(p => playerIds.Contains(p.PlayerId)).Select(p => new { PlayerName = p.PlayerName.UrlDecode(), p.PlayerId }).ToListAsync();

            return Ok(players);
        }

        [HttpGet("request-backline-defense")]
        public async Task<IActionResult> GetAvailableBacklineDefense(short x, short y, int? maxTravelSeconds)
        {
            (var currentVillages, var enemyVillages) = await ManyTasks.RunToList(
                from user in CurrentSets.ActiveUser
                join village in CurrentSets.Village on user.PlayerId equals village.PlayerId
                join currentVillage in CurrentSets.CurrentVillage.Include(cv => cv.ArmyAtHome).Include(cv => cv.ArmyOwned)
                    on village.VillageId equals currentVillage.VillageId
                where currentVillage.ArmyAtHomeId != null && currentVillage.ArmyOwnedId != null
                select new { Village = village, CurrentVillage = currentVillage }
                
                ,

                from enemy in CurrentSets.EnemyTribe
                join player in CurrentSets.Player on enemy.EnemyTribeId equals player.TribeId
                join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                select new Coordinate { X = village.X.Value, Y = village.Y.Value }
            );

            //  Ignore current villages where < 10% of their troops are at home
            currentVillages = currentVillages
                // + 1 on army owned so we don't divide by 0 accidentally
                .Where(v => ArmyStats.CalculateTotalPopulation(v.CurrentVillage.ArmyAtHome) / (float)(ArmyStats.CalculateTotalPopulation(v.CurrentVillage.ArmyOwned) + 1) > 0.2f)
                .ToList();

            var enemyMap = new Features.Spatial.Quadtree(enemyVillages);
            var backlineSupport = currentVillages
                .Where(s => !enemyMap.ContainsInRange(s.Village.X.Value, s.Village.Y.Value, 10))
                .ToDictionary(s => s.Village, s => s.CurrentVillage);

            var planner = new Features.Planning.CommandOptionsCalculator(CurrentWorldSettings);
            planner.Requirements.Add(new MinimumDefenseRequirement { MinimumDefense = 10000 }.LimitTroopType(ArmyStats.DefensiveTroopTypes));
            if (maxTravelSeconds != null)
                planner.Requirements.Add(new MaximumTravelTimeRequirement { MaximumTime = TimeSpan.FromSeconds(maxTravelSeconds.Value) });

            var options = planner.GenerateOptions(backlineSupport, new Village { X = x, Y = y });

            var villageIds = options.Select(o => o.SendFrom).Distinct().ToList();
            var playerIds = await CurrentSets.Village.Where(v => villageIds.Contains(v.VillageId)).Select(v => v.PlayerId.Value).Distinct().ToListAsync();
            if (playerIds.Contains(CurrentPlayerId))
                playerIds.Remove(CurrentPlayerId);


            var players = await CurrentSets.Player.Where(p => playerIds.Contains(p.PlayerId)).Select(p => new { PlayerName = p.PlayerName.UrlDecode(), p.PlayerId }).ToListAsync();

            return Ok(players);
        }
    }
}