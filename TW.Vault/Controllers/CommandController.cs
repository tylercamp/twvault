using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;

using JSON = TW.Vault.Model.JSON;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using TW.Vault.Model.Validation;
using TW.Vault.Model;
using Native = TW.Vault.Model.Native;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Command")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class CommandController : BaseController
    {
        public CommandController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }
        
        [HttpGet("{id}", Name = "Get")]
        public Task<IActionResult> Get(long id)
        {
            return SelectOr404<Scaffold.Command>(
                q => q.Where(c => c.CommandId == id).FromWorld(CurrentWorldId),
                c => CommandConvert.ModelToJson(c)
            );
        }

        [HttpGet("village/target/{villageId}")]
        public async Task<IActionResult> GetByTargetVillage(long villageId)
        {
            var commands = await Paginated(
                    from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                    where command.TargetVillageId == villageId
                    select command
                ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("village/source/{villageId}")]
        public async Task<IActionResult> GetBySourceVillage(long villageId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.SourceVillageId == villageId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("player/target/{playerId}")]
        public async Task<IActionResult> GetByTargetPlayer(long playerId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.TargetPlayerId == playerId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("player/source/{playerId}")]
        public async Task<IActionResult> GetBySourcePlayer(long playerId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.SourcePlayerId == playerId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpPost("check-existing-commands")]
        public async Task<IActionResult> GetExistingCommands([FromBody]List<long> commandIds)
        {
            var existingIds = await (
                    from command in context.Command.FromWorld(CurrentWorldId)
                    where command.ArmyId != null
                    where commandIds.Contains(command.CommandId)
                    select command.CommandId
                ).ToListAsync();

            return Ok(existingIds);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JSON.ManyCommands jsonCommands)
        {
            if (ModelState.IsValid)
            {
                var mappedCommands = jsonCommands.Commands.ToDictionary(c => c.CommandId, c => c);
                var commandIds = jsonCommands.Commands.Select(c => c.CommandId).ToList();

                var allVillageIds = jsonCommands.Commands
                                .Select(c => c.SourceVillageId)
                                .Concat(jsonCommands.Commands.Select(c => c.TargetVillageId))
                                .Select(id => id.Value)
                                .Distinct();

                var villageIdsFromCommandsMissingTroopType = jsonCommands.Commands
                    .Where(c => c.TroopType == null)
                    .SelectMany(c => new[] { c.SourceVillageId, c.TargetVillageId })
                    .Distinct()
                    .ToList();

                var (scaffoldCommands, villageIdsFromCommandsMissingTroopTypes, allVillages) = await ManyTasks.RunToList(
                    from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                    where commandIds.Contains(command.CommandId)
                    select command
                    
                    ,

                    from village in context.Village.FromWorld(CurrentWorldId)
                    where villageIdsFromCommandsMissingTroopType.Contains(village.VillageId)
                    select village

                    ,

                    from village in context.Village.FromWorld(CurrentWorldId)
                    where allVillageIds.Contains(village.VillageId)
                    select village
                );
                

                var mappedScaffoldCommands = scaffoldCommands.ToDictionary(c => c.CommandId, c => c);
                var villagesById = allVillages.ToDictionary(v => v.VillageId, v => v);

                var tx = BuildTransaction();
                context.Transaction.Add(tx);

                Profile("Generate scaffold commands", () =>
                {
                    foreach (var jsonCommand in jsonCommands.Commands)
                    {
                        if (!Configuration.Security.AllowCommandArrivalBeforeServerTime
                                && jsonCommand.LandsAt.HasValue
                                && jsonCommand.LandsAt.Value < CurrentServerTime)
                        {
                            context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                                JsonConvert.SerializeObject(jsonCommand),
                                "Command.landsAt is earlier than current server time"
                            ));
                            continue;
                        }

                        if (!Configuration.Security.ReportIgnoreExpectedPopulationBounds
                                && !ArmyValidate.MeetsPopulationRestrictions(jsonCommand.Troops))
                        {
                            context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                                JsonConvert.SerializeObject(jsonCommand),
                                "Troops in command exceed possible village population"
                            ));
                            continue;
                        }


                        var travelCalculator = new Features.Simulation.TravelCalculator(2.0f, 0.5f);
                        var timeRemaining = jsonCommand.LandsAt.Value - CurrentServerTime;
                        var sourceVillage = villagesById[jsonCommand.SourceVillageId.Value];
                        var targetVillage = villagesById[jsonCommand.TargetVillageId.Value];
                        var estimatedType = travelCalculator.EstimateTroopType(timeRemaining, sourceVillage, targetVillage);

                        if (jsonCommand.TroopType == null)
                        {
                            jsonCommand.TroopType = estimatedType;
                        }
                        else
                        {
                            var estimatedTravelSpeed = Native.ArmyStats.TravelSpeed[estimatedType];
                            var reportedTravelSpeed = Native.ArmyStats.TravelSpeed[jsonCommand.TroopType.Value];

                            //  ie if command is tagged as "spy" but travel speed is effective for
                            //  rams
                            if (estimatedTravelSpeed > reportedTravelSpeed)
                                jsonCommand.TroopType = estimatedType;
                        }

                        var scaffoldCommand = mappedScaffoldCommands.GetValueOrDefault(jsonCommand.CommandId.Value);
                        //  Don't process/update commands that are already "complete" (have proper army data attached to them)
                        if (scaffoldCommand?.Army != null)
                            continue;

                        if (scaffoldCommand == null)
                        {
                            scaffoldCommand = new Scaffold.Command();
                            scaffoldCommand.World = CurrentWorld;
                            jsonCommand.ToModel(scaffoldCommand, context);
                            context.Command.Add(scaffoldCommand);
                        }
                        else
                        {
                            var existingJsonCommand = CommandConvert.ModelToJson(scaffoldCommand);
                            if (existingJsonCommand.IsReturning == jsonCommand.IsReturning && existingJsonCommand != jsonCommand)
                            {
                                context.ConflictingDataRecord.Add(new Scaffold.ConflictingDataRecord
                                {
                                    OldTxId = scaffoldCommand.TxId.Value,
                                    ConflictingTx = tx
                                });
                            }

                            jsonCommand.ToModel(scaffoldCommand, context);
                        }

                        scaffoldCommand.Tx = tx;
                    }
                });

                var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUser.Uid);
                if (jsonCommands.IsOwnCommands.Value)
                    userUploadHistory.LastUploadedCommandsAt = DateTime.UtcNow;
                else
                    userUploadHistory.LastUploadedIncomingsAt = DateTime.UtcNow;

                await Profile("Save changes", () => context.SaveChangesAsync());

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("tags", Name = "GetIncomingTags")]
        public async Task<IActionResult> GetIncomingTags([FromBody]List<long> incomingsIds)
        {
            var incomings = await (
                    from command in context.Command.FromWorld(CurrentWorldId)
                                                   .Include(c => c.SourceVillage)
                                                   .Include(c => c.SourceVillage.CurrentVillage)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyOwned)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyStationed)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyTraveling)
                    where incomingsIds.Contains(command.CommandId)
                    select command
                ).ToListAsync();

            if (incomings == null)
                return NotFound();

            var commandSourceVillageIds = incomings.Select(inc => inc.SourceVillageId).ToList();

            var countsByVillage = commandSourceVillageIds.Distinct().ToDictionary(
                vid => vid,
                vid => commandSourceVillageIds.Count(sv => sv == vid)
            );

            var commandsReturningByVillageId = await Profile("Get returning commands for all source villages", async () =>
            {
                var commandsReturning = await (
                    from command in context.Command
                                            .FromWorld(CurrentWorldId)
                                            .Include(c => c.Army)
                    where commandSourceVillageIds.Contains(command.SourceVillageId)
                    where command.IsReturning
                    where command.LandsAt < CurrentServerTime
                    select command
                ).ToListAsync();

                return commandSourceVillageIds.ToDictionary(
                    vid => vid,
                    vid => commandsReturning.Where(cmd => cmd.SourceVillageId == vid).ToList()
                );
            });

            Dictionary<long, JSON.IncomingTag> resultTags = new Dictionary<long, JSON.IncomingTag>();
            Profile("Make incomings tags", () =>
            {
                foreach (var incoming in incomings)
                {
                    var sourceCurrentVillage = incoming.SourceVillage?.CurrentVillage;
                    var sourceVillageId = incoming.SourceVillageId;
                    var commandsReturning = commandsReturningByVillageId.GetValueOrDefault(sourceVillageId);

                    var armyOwned = sourceCurrentVillage?.ArmyOwned;
                    var armyTraveling = sourceCurrentVillage?.ArmyTraveling;
                    var armyStationed = sourceCurrentVillage?.ArmyStationed;

                    //  TODO - Make this a setting
                    var maxUpdateTime = TimeSpan.FromDays(4);

                    if (armyOwned?.LastUpdated != null && (CurrentServerTime - armyOwned.LastUpdated.Value > maxUpdateTime))
                        armyOwned = null;

                    if (armyTraveling?.LastUpdated != null && (CurrentServerTime - armyTraveling.LastUpdated.Value > maxUpdateTime))
                        armyTraveling = null;

                    if (armyStationed?.LastUpdated != null && (CurrentServerTime - armyStationed.LastUpdated.Value > maxUpdateTime))
                        armyStationed = null;

                    if (armyOwned != null && armyOwned.IsEmpty())
                        armyOwned = null;
                    if (armyTraveling != null && armyTraveling.IsEmpty())
                        armyTraveling = null;
                    if (armyStationed != null && armyStationed.IsEmpty())
                        armyStationed = null;

                    var troopsReturning = new JSON.Army();
                    if (commandsReturning != null)
                    {
                        foreach (var command in commandsReturning)
                            troopsReturning += ArmyConvert.ArmyToJson(command.Army);
                    }

                    var effectiveArmy = armyOwned ?? armyTraveling ?? armyStationed;

                    var tag = new JSON.IncomingTag();
                    tag.CommandId = incoming.CommandId;
                    tag.NumFromVillage = countsByVillage.GetValueOrDefault(sourceVillageId);
                    tag.TroopType = TroopTypeConvert.StringToTroopType(incoming.TroopType);

                    if (effectiveArmy != null)
                    {
                        //  TODO - Make this a setting
                        bool isOffense = (
                                (effectiveArmy.Axe.HasValue && effectiveArmy.Axe.Value > 500) ||
                                (effectiveArmy.Light.HasValue && effectiveArmy.Light.Value > 250)
                            );

                        if (!isOffense)
                            tag.DefiniteFake = true;

                        var offensiveArmy = effectiveArmy.OfType(JSON.UnitBuild.Offensive);
                        var jsonArmy = ArmyConvert.ArmyToJson(offensiveArmy);
                        var pop = Native.ArmyStats.CalculateTotalPopulation(jsonArmy);

                        var returningOffensiveArmy = troopsReturning.OfType(JSON.UnitBuild.Offensive);
                        var returningPop = Native.ArmyStats.CalculateTotalPopulation(returningOffensiveArmy);

                        tag.OffensivePopulation = pop - returningPop;
                        if (pop - returningPop < 1000)
                            tag.DefiniteFake = true;

                        tag.NumCats = effectiveArmy.Catapult;
                    }

                    resultTags.Add(incoming.CommandId, tag);
                }
            });

            return Ok(resultTags);
        }
    }
}
