using System;
using System.Collections.Concurrent;
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
using TW.Vault.Features.Planning.Requirements.Modifiers;
using TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;
using Planning = TW.Vault.Features.Planning;


namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Plan")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlanController : BaseController
    {
        public PlanController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet("backtime-list")]
        public async Task<IActionResult> GetBacktimePlan()
        {
            var serverTime = CurrentServerTime;

            var invalidTribeIds = await (
                    from tribe in CurrentSets.Ally
                    join player in CurrentSets.Player on tribe.TribeId equals player.TribeId
                    join user in CurrentSets.User on player.PlayerId equals user.PlayerId
                    where user.Enabled
                    select tribe.TribeId
                ).Distinct().ToListAsync();

            var invalidPlayerIds = await (
                    from player in CurrentSets.Player
                    join user in CurrentSets.User on player.PlayerId equals user.PlayerId into user
                    where user.Any(u => u.Enabled) || (player.TribeId != null && invalidTribeIds.Contains(player.TribeId.Value))
                    select player.PlayerId
                ).ToListAsync();

            var invalidVillageIds = await (
                    from village in CurrentSets.Village
                    where village.PlayerId != null && invalidPlayerIds.Contains(village.PlayerId.Value)
                    select village.VillageId
                ).ToListAsync();

            var ownVillages = await (
                    from village in CurrentSets.Village
                    join currentVillage in CurrentSets.CurrentVillage.Include(cv => cv.ArmyAtHome) on village.VillageId equals currentVillage.VillageId
                    where currentVillage.ArmyAtHome != null
                    where village.PlayerId == CurrentPlayerId
                    select new { village, currentVillage }
                ).ToDictionaryAsync(d => d.village, d => d.currentVillage);

            var possibleCommands = await (
                    from command in CurrentSets.Command
                                               .Include(c => c.Army)
                                               .Include(c => c.SourceVillage)
                    where command.ArmyId != null
                    where command.ReturnsAt > serverTime
                    where !invalidVillageIds.Contains(command.SourceVillageId)
                    select command
                ).ToListAsync();

            var offensiveTypes = new[] { JSON.TroopType.Axe, JSON.TroopType.Light, JSON.TroopType.Heavy, JSON.TroopType.Ram, JSON.TroopType.Catapult };
            var allInstructions = new ConcurrentDictionary<Scaffold.Command, List<Planning.CommandInstruction>>();

            var ownVillagesById = ownVillages.Keys.ToDictionary(v => v.VillageId, v => v);

            bool MeetsMinimumPopulation(Scaffold.Command command)
            {
                var army = (JSON.Army)command.Army;
                return army != null && 2000 < Model.Native.ArmyStats.CalculateTotalPopulation(army, offensiveTypes);
            }

            var targetPlayerIdsTmp = new ConcurrentDictionary<long, byte>();

            //  Generate backtime plans
            try
            {
                var commands = possibleCommands.Where(MeetsMinimumPopulation);
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                var planningTask = Parallel.ForEach(commands, parallelOptions, (command) =>
                {
                    var planner = new Planning.CommandOptionsCalculator(CurrentWorldSettings);
                    planner.Requirements.Add(new Planning.Requirements.MaximumTravelTimeRequirement
                    {
                        MaximumTime = command.ReturnsAt.Value - serverTime
                    });

                    planner.Requirements.Add(Planning.Requirements.MinimumOffenseRequirement.HalfNuke.LimitTroopType(offensiveTypes));
                    planner.Requirements.Add(Planning.Requirements.WithoutTroopTypeRequirement.WithoutNobles);

                    var plan = planner.GenerateOptions(ownVillages, command.SourceVillage);
                    if (plan.Count > 0)
                    {
                        targetPlayerIdsTmp.TryAdd(command.SourcePlayerId, 0);
                        allInstructions.TryAdd(command, plan);
                    }
                });
            }
            catch (AggregateException e)
            {
                throw e.InnerException ?? e.InnerExceptions.First();
            }

            //  Find existing backtimes for commands that had plans generated
            var backtimedVillageIds = allInstructions.Keys.Select(c => c.SourceVillageId).ToList();
            var commandsToBacktimedVillages = await (
                    from command in CurrentSets.Command
                    where backtimedVillageIds.Contains(command.TargetVillageId)
                    where command.Army != null
                    where command.LandsAt > serverTime
                    select new { command.TargetVillageId, command.LandsAt, command.Army }
                ).ToListAsync();

            var troopsAtBacktimedVillages = await (
                    from village in CurrentSets.CurrentVillage
                    where backtimedVillageIds.Contains(village.VillageId)
                    select new { village.VillageId, village.ArmyStationed }
                ).ToDictionaryAsync(v => v.VillageId, v => v.ArmyStationed);

            var battleSimulator = new Features.Simulation.BattleSimulator();
            var existingBacktimesPerCommand = allInstructions.Keys.ToDictionary(c => c.CommandId, id => 0);
            foreach (var command in commandsToBacktimedVillages)
            {
                Scaffold.Command backtimedCommand = null;
                var commandsReturningToVillage = allInstructions.Keys.Where(c => c.SourceVillageId == command.TargetVillageId);
                foreach (var returning in commandsReturningToVillage)
                {
                    if (command.LandsAt > returning.ReturnsAt && (command.LandsAt - returning.ReturnsAt.Value).TotalSeconds < 10)
                        backtimedCommand = returning;
                }

                if (backtimedCommand == null)
                    continue;

                var backtimedArmy = (JSON.Army)backtimedCommand.Army;
                var battleResult = battleSimulator.SimulateAttack(command.Army, backtimedArmy, 20, CurrentWorldSettings.ArchersEnabled);
                var originalPopulation = (float)Model.Native.ArmyStats.CalculateTotalPopulation(backtimedArmy, offensiveTypes);
                var newPopulation = (float)Model.Native.ArmyStats.CalculateTotalPopulation(battleResult.DefendingArmy, offensiveTypes);

                var percentLost = 1 - newPopulation / originalPopulation;
                if (percentLost > 0.85f)
                    existingBacktimesPerCommand[backtimedCommand.CommandId]++;
            }

            var targetPlayerIds = targetPlayerIdsTmp.Keys.ToList();

            var playerInfoById = await (
                    from player in CurrentSets.Player
                    where targetPlayerIds.Contains(player.PlayerId)
                    select new { player.PlayerId, player.TribeId, player.PlayerName }
                ).ToDictionaryAsync(i => i.PlayerId, i => i);

            var tribeIds = playerInfoById.Values.Select(i => i.TribeId).Where(t => t != null).Distinct();
            var tribeInfoById = await (
                    from tribe in CurrentSets.Ally
                    where tribeIds.Contains(tribe.TribeId)
                    select new { tribe.TribeId, tribe.Tag, tribe.TribeName }
                ).ToDictionaryAsync(i => i.TribeId, i => i);

            //  Get JSON version of instructions and info for all backtimeable nukes
            var result = allInstructions.Select(commandInstructions =>
            {
                (var command, var instructions) = commandInstructions.Tupled();

                var backtimeInfo = new JSON.BacktimeInfo();
                var targetVillage = command.SourceVillage; // "Target" is the source of the command
                var targetPlayer = playerInfoById[command.SourcePlayerId];
                var targetTribe = targetPlayer.TribeId == null ? null : tribeInfoById[targetPlayer.TribeId.Value];
                var isStacked = false;
                if (troopsAtBacktimedVillages[targetVillage.VillageId] != null)
                {
                    var stationedTroops = troopsAtBacktimedVillages[targetVillage.VillageId];
                    var defensePower = Features.Simulation.BattleSimulator.TotalDefensePower(stationedTroops);
                    isStacked = defensePower > 1000000;
                }

                //  Gather instructions and info for this command
                return new JSON.BacktimeInfo
                {
                    //  General info
                    TravelingArmyPopulation = Model.Native.ArmyStats.CalculateTotalPopulation(command.Army, offensiveTypes),
                    TargetPlayerName = targetPlayer.PlayerName.UrlDecode(),
                    TargetTribeName = targetTribe?.TribeName?.UrlDecode(),
                    TargetTribeTag = targetTribe?.Tag?.UrlDecode(),
                    ExistingBacktimes = existingBacktimesPerCommand[command.CommandId],
                    IsStacked = isStacked,

                    //  Convert backtime instructions to JSON format
                    Instructions = instructions.Select(instruction =>
                    {
                        var sourceVillage = ownVillagesById[instruction.SendFrom];
                        var sourceVillageArmy = (JSON.Army)ownVillages[sourceVillage].ArmyAtHome;
                        var instructionArmy = sourceVillageArmy.BasedOn(instruction.TroopType);

                        return new JSON.BattlePlanCommand
                        {
                            LandsAt = command.ReturnsAt.Value,
                            LaunchAt = command.ReturnsAt.Value - instruction.TravelTime,
                            TravelTimeSeconds = (int)instruction.TravelTime.TotalSeconds,

                            TroopType = instruction.TroopType.ToString().ToLower(),
                            CommandPopulation = Model.Native.ArmyStats.CalculateTotalPopulation(instructionArmy),
                            CommandAttackPower = Features.Simulation.BattleSimulator.TotalAttackPower(instructionArmy),
                            CommandDefensePower = Features.Simulation.BattleSimulator.TotalDefensePower(instructionArmy),

                            SourceVillageId = instruction.SendFrom,
                            TargetVillageId = instruction.SendTo,

                            SourceVillageName = sourceVillage.VillageName.UrlDecode(),
                            TargetVillageName = targetVillage.VillageName.UrlDecode(),

                            SourceVillageX = sourceVillage.X.Value,
                            SourceVillageY = sourceVillage.Y.Value,

                            TargetVillageX = targetVillage.X.Value,
                            TargetVillageY = targetVillage.Y.Value,
                        };
                    }).ToList()
                };

            }).ToList();

            return Ok(result);
        }
    }
}