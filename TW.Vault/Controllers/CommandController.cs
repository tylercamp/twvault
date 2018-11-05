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
using TW.Vault.Features.Planning.Requirements;
using TW.Vault.Features.Planning;
using System.Net;
using TW.Vault.Features.Simulation;

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

        [HttpPost("finished-command-uploads")]
        public async Task<IActionResult> SetUserFinishedCommandUploads()
        {
            var history = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
            history.LastUploadedCommandsAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("finished-incoming-uploads")]
        public async Task<IActionResult> SetUserFinishedIncomingUploads()
        {
            var history = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
            history.LastUploadedIncomingsAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok();
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

                var (scaffoldCommands, villageIdsFromCommandsMissingTroopTypes, allVillages) = await ManyTasks.Run(
                    Profile("Get existing commands", () => (
                        from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                        where commandIds.Contains(command.CommandId)
                        select command
                    ).ToListAsync())
                    
                    ,

                    Profile("Get villages for commands missing troop type", () => (
                        from village in context.Village.FromWorld(CurrentWorldId)
                        where villageIdsFromCommandsMissingTroopType.Contains(village.VillageId)
                        select village
                    ).ToListAsync())

                    ,

                    Profile("Get all relevant villages", () => (
                        from village in context.Village.FromWorld(CurrentWorldId)
                        where allVillageIds.Contains(village.VillageId)
                        select village
                    ).ToListAsync())
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

                        var scaffoldCommand = mappedScaffoldCommands.GetValueOrDefault(jsonCommand.CommandId.Value);
                        //  Don't process/update commands that are already "complete" (have proper army data attached to them)
                        if (scaffoldCommand?.Army != null)
                            continue;

                        var travelCalculator = new Features.Simulation.TravelCalculator(CurrentWorldSettings.GameSpeed, CurrentWorldSettings.UnitSpeed);
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

                        if (scaffoldCommand == null)
                        {
                            scaffoldCommand = new Scaffold.Command();
                            scaffoldCommand.World = CurrentWorld;
                            jsonCommand.ToModel(CurrentWorldId, scaffoldCommand, context);
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

                            jsonCommand.ToModel(CurrentWorldId, scaffoldCommand, context);
                        }

                        if (String.IsNullOrWhiteSpace(scaffoldCommand.UserLabel) || scaffoldCommand.UserLabel == "Attack")
                            scaffoldCommand.UserLabel = scaffoldCommand.TroopType.Capitalized();

                        if (jsonCommand.TroopType != null)
                        {
                            var travelTime = travelCalculator.CalculateTravelTime(jsonCommand.TroopType.Value, sourceVillage, targetVillage);
                            scaffoldCommand.ReturnsAt = scaffoldCommand.LandsAt + travelTime;
                        }

                        scaffoldCommand.Tx = tx;
                    }
                });

                await Profile("Save changes", () => context.SaveChangesAsync());

                //  Run upload history update in separate query to prevent creating multiple history
                //  entries
                var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
                if (jsonCommands.IsOwnCommands.Value)
                    userUploadHistory.LastUploadedCommandsAt = DateTime.UtcNow;
                else
                    userUploadHistory.LastUploadedIncomingsAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

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
            // Preload world data since we need world settings within queries below
            LoadWorldData();
            //  Lots of data read but only updating some of it; whenever we do SaveChanges it checks
            //  for changes against all queried objects. Disable tracking by default and track explicitly if necessary
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var incomings = await Profile("Get existing commands", () => (
                    from command in context.Command.FromWorld(CurrentWorldId)
                                                   .Include(c => c.SourceVillage)
                                                   .Include(c => c.SourceVillage.CurrentVillage)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyOwned)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyStationed)
                                                   .Include(c => c.SourceVillage.CurrentVillage.ArmyTraveling)
                    where incomingsIds.Contains(command.CommandId)
                    select command
                ).ToListAsync()
            );

            if (incomings == null)
                return NotFound();

            var uploadHistory = await Profile("Get user upload history", () =>
                context.UserUploadHistory.Where(h => h.Uid == CurrentUserId).FirstOrDefaultAsync()
            );

            var validationInfo = UploadRestrictionsValidate.ValidateInfo.FromTaggingRestrictions(CurrentUser, uploadHistory);
            List<String> needsUpdateReasons = UploadRestrictionsValidate.GetNeedsUpdateReasons(DateTime.UtcNow, validationInfo);

            if (needsUpdateReasons != null && needsUpdateReasons.Any())
            {
                return StatusCode(423, needsUpdateReasons); // Status code "Locked"
            }

            //  NOTE - We pull data for all villas requested but only return data for villas not in vaultOwnedVillages,
            //  should stop querying that other data at some point
            var commandSourceVillageIds = incomings.Select(inc => inc.SourceVillageId).Distinct().ToList();
            var commandTargetVillageIds = incomings.Select(inc => inc.TargetVillageId).Distinct().ToList();

            var relevantVillages = await (
                    from village in context.Village.FromWorld(CurrentWorldId)
                    where commandSourceVillageIds.Contains(village.VillageId) || commandTargetVillageIds.Contains(village.VillageId)
                    select new { village.PlayerId, village.VillageId, village.VillageName, X = village.X.Value, Y = village.Y.Value }
                ).ToDictionaryAsync(v => v.VillageId, v => v);

            var sourcePlayerIds = relevantVillages.Values.Where(v => commandSourceVillageIds.Contains(v.VillageId)).Select(v => v.PlayerId ?? 0).ToList();

            (var vaultOwnedVillages, var sourcePlayerNames, var countsByVillage) = await ManyTasks.Run(
                //  Don't do any tagging for villages owned by players registered with the vault (so players in other tribes
                //  also using the vault can't infer villa builds)
                Profile("Get villages owned by vault users", () => (
                    from user in context.User
                    join village in context.Village.FromWorld(CurrentWorldId) on user.PlayerId equals village.PlayerId
                    where user.Enabled
                    select village.VillageId
                ).ToListAsync())

                ,

                Profile("Get player names", () => (
                    from player in context.Player.FromWorld(CurrentWorldId)
                    where sourcePlayerIds.Contains(player.PlayerId)
                    select new { player.PlayerId, player.PlayerName }
                ).ToDictionaryAsync(p => p.PlayerId, p => p.PlayerName))

                ,

                Profile("Get command counts", () => (
                    from command in context.Command.FromWorld(CurrentWorldId)
                    where !command.IsReturning && command.LandsAt > CurrentServerTime
                    group command by command.SourceVillageId into villageCommands
                    select new { VillageId = villageCommands.Key, Count = villageCommands.Count() }
                ).ToDictionaryAsync(vc => vc.VillageId, vc => vc.Count))

            );
            

            var travelCalculator = new Features.Simulation.TravelCalculator(CurrentWorldSettings.GameSpeed, CurrentWorldSettings.UnitSpeed);
            DateTime CommandLaunchedAt(Scaffold.Command command) => command.LandsAt - travelCalculator.CalculateTravelTime(
                (command.TroopType ?? "ram").ToTroopType(),
                relevantVillages[command.SourceVillageId].X, relevantVillages[command.SourceVillageId].Y,
                relevantVillages[command.TargetVillageId].X, relevantVillages[command.TargetVillageId].Y
            );

            var earliestLaunchTime = incomings.Select(CommandLaunchedAt).DefaultIfEmpty(DateTime.UtcNow).Min() - CurrentWorldSettings.UtcOffset;

            var commandsReturningByVillageId = await Profile("Process returning commands for all source villages", async () =>
            {
                var commandSeenThreshold = earliestLaunchTime - TimeSpan.FromDays(1);

                var sentCommands = await Profile("Query returning commands for all source villages", () => (
                    from command in context.Command.AsTracking()
                                            .FromWorld(CurrentWorldId)
                                            .Include(c => c.Army)
                    where command.FirstSeenAt > commandSeenThreshold
                    where command.Army != null
                    where commandSourceVillageIds.Contains(command.SourceVillageId)
                    select command
                ).ToListAsync());

                bool updatedCommands = false;
                var result = commandSourceVillageIds.ToDictionary(vid => vid, vid => new List<Scaffold.Command>());

                Profile("Update command returning and sort into dictionary", () =>
                {
                    foreach (var cmd in sentCommands)
                    {
                        if (cmd.LandsAt <= CurrentServerTime)
                        {
                            if (!cmd.IsReturning)
                            {
                                updatedCommands = true;
                                cmd.IsReturning = true;
                            }

                            result[cmd.SourceVillageId].Add(cmd);
                        }
                    }
                });

                if (updatedCommands)
                    await Profile("Save commands now set to returning", () => context.SaveChangesAsync());

                return result;
            });

            var otherTargetedVillageIds = commandsReturningByVillageId.SelectMany(kvp => kvp.Value).Select(c => c.TargetVillageId).Distinct().Except(relevantVillages.Keys);
            var otherTargetVillages = await Profile("Get other villages targeted by inc source villas", () =>
                context.Village.FromWorld(CurrentWorldId).Where(v => otherTargetedVillageIds.Contains(v.VillageId)).ToListAsync()
            );

            foreach (var id in otherTargetedVillageIds)
            {
                var village = otherTargetVillages.First(v => v.VillageId == id);
                relevantVillages.Add(id, new
                {
                    village.PlayerId, village.VillageId, village.VillageName, X = village.X.Value, Y = village.Y.Value
                });
            }

            var launchTimesByCommandId = commandsReturningByVillageId.SelectMany(kvp => kvp.Value).Where(cmd => !vaultOwnedVillages.Contains(cmd.SourceVillageId)).ToDictionary(
                cmd => cmd.CommandId,
                cmd => CommandLaunchedAt(cmd)
            );

            IEnumerable<Scaffold.Command> RelevantCommandsForIncoming(Scaffold.Command incoming)
            {
                if (!relevantVillages.ContainsKey(incoming.SourceVillageId))
                    return Enumerable.Empty<Scaffold.Command>();

                var launchTime = CommandLaunchedAt(incoming);
                var returningCommands = commandsReturningByVillageId.GetValueOrDefault(incoming.SourceVillageId);
                if (returningCommands == null)
                    return Enumerable.Empty<Scaffold.Command>();

                return returningCommands.Where(cmd => cmd.ReturnsAt > launchTime || (launchTimesByCommandId.ContainsKey(cmd.CommandId) && launchTimesByCommandId[cmd.CommandId] > launchTime));
            }

            Dictionary<long, JSON.IncomingTag> resultTags = new Dictionary<long, JSON.IncomingTag>();
            Profile("Make incomings tags", () =>
            {
                foreach (var incoming in incomings)
                {
                    var sourceVillageId = incoming.SourceVillageId;
                    if (vaultOwnedVillages.Contains(sourceVillageId))
                        continue;

                    var sourceCurrentVillage = incoming.SourceVillage?.CurrentVillage;
                    var commandsReturning = RelevantCommandsForIncoming(incoming);

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

                    Scaffold.CurrentArmy effectiveArmy = null;
                    bool isConfidentArmy = true;
                    if (armyOwned != null)
                    {
                        effectiveArmy = armyOwned;
                    }
                    else if (armyTraveling != null)
                    {
                        effectiveArmy = armyTraveling;
                    }
                    else if (armyStationed != null)
                    {
                        effectiveArmy = armyStationed;
                        isConfidentArmy = false;
                    }

                    var tag = new JSON.IncomingTag();
                    tag.CommandId = incoming.CommandId;
                    tag.OriginalTag = incoming.UserLabel;
                    tag.NumFromVillage = countsByVillage.GetValueOrDefault(sourceVillageId);
                    tag.TroopType = TroopTypeConvert.StringToTroopType(incoming.TroopType);

                    var sourceVillage = relevantVillages[incoming.SourceVillageId];
                    var targetVillage = relevantVillages[incoming.TargetVillageId];
                    tag.SourceVillageCoords = $"{sourceVillage.X}|{sourceVillage.Y}";
                    tag.TargetVillageCoords = $"{targetVillage.X}|{targetVillage.Y}";
                    tag.SourcePlayerName = WebUtility.UrlDecode(sourcePlayerNames[incoming.SourcePlayerId]);
                    tag.SourceVillageName = WebUtility.UrlDecode(sourceVillage.VillageName);
                    tag.TargetVillageName = WebUtility.UrlDecode(targetVillage.VillageName);
                    tag.Distance = new Coordinate { X = sourceVillage.X, Y = sourceVillage.Y }.DistanceTo(targetVillage.X, targetVillage.Y);

                    if (effectiveArmy != null)
                    {
                        //  TODO - Make this a setting
                        bool isOffense = (
                                (effectiveArmy.Axe.HasValue && effectiveArmy.Axe.Value > 500) ||
                                (effectiveArmy.Light.HasValue && effectiveArmy.Light.Value > 250)
                            );

                        tag.VillageType = isOffense ? "Offense" : "Defense";

                        if (!isOffense && isConfidentArmy && (effectiveArmy.Snob == null || effectiveArmy.Snob == 0) && incoming.TroopType != JSON.TroopType.Snob.ToTroopString())
                            tag.DefiniteFake = true;

                        var offensiveArmy = effectiveArmy.OfType(JSON.UnitBuild.Offensive);
                        var jsonArmy = ArmyConvert.ArmyToJson(offensiveArmy);
                        var pop = Native.ArmyStats.CalculateTotalPopulation(jsonArmy);

                        var returningOffensiveArmy = troopsReturning.OfType(JSON.UnitBuild.Offensive);
                        var returningPop = Native.ArmyStats.CalculateTotalPopulation(returningOffensiveArmy);

                        tag.OffensivePopulation = pop - returningPop;
                        if (tag.OffensivePopulation < 0)
                            tag.OffensivePopulation = 0;

                        if ((tag.OffensivePopulation > 100 || returningPop > 5000) && tag.OffensivePopulation < 5000 && isConfidentArmy)
                            tag.DefiniteFake = true;

                        tag.ReturningPopulation = returningPop;

                        tag.NumCats = effectiveArmy.Catapult;
                    }

                    resultTags.Add(incoming.CommandId, tag);
                }
            });

            return Ok(resultTags);
        }

        [HttpGet("{commandId}/backtime")]
        public async Task<IActionResult> MakeBacktimePlan(long commandId)
        {
            var command = await context.Command
                                       .FromWorld(CurrentWorldId)
                                       .Where(cmd => cmd.CommandId == commandId)
                                       .Include(cmd => cmd.SourceVillage)
                                       .FirstOrDefaultAsync();

            if (command == null)
                return NotFound();

            var options = new List<JSON.BattlePlanCommand>();

            if (command.ReturnsAt == null)
                return Ok(options);




            var targetVillage = await context.Village.FromWorld(CurrentWorldId).Where(v => v.VillageId == command.SourceVillageId).FirstOrDefaultAsync();

            var availableVillages = await Profile("Get available villages", () => (
                from currentVillage in context.CurrentVillage.FromWorld(CurrentWorldId).Include(cv => cv.Village).Include(cv => cv.ArmyAtHome)
                where currentVillage.Village.PlayerId == CurrentPlayerId
                select currentVillage
            ).ToListAsync());

            var villagesById = availableVillages.ToDictionary(cv => cv.VillageId, cv => cv);

            var planner = new CommandOptionsCalculator(CurrentWorldSettings.GameSpeed, CurrentWorldSettings.UnitSpeed);

            planner.Requirements.Add(new MaximumTravelTimeRequirement
            {
                MaximumTime = command.ReturnsAt.Value - CurrentServerTime
            });

            planner.Requirements.Add(MinimumOffenseRequirement.HalfNuke);

            var instructions = Profile("Generate plan", () => planner.GenerateOptions(
                availableVillages.ToDictionary(cv => cv.Village, cv => cv),
                targetVillage
            ));

            foreach (var instruction in instructions)
            {
                var sourceVillageArmy = villagesById[instruction.SendFrom].ArmyAtHome;
                var instructionArmy = ((JSON.Army)sourceVillageArmy).BasedOn(instruction.TroopType);


                
                options.Add(new JSON.BattlePlanCommand
                {
                    LandsAt = command.ReturnsAt,
                    LaunchAt = command.ReturnsAt - instruction.TravelTime,
                    TravelTimeSeconds = (int)instruction.TravelTime.TotalSeconds,
                    TroopType = instruction.TroopType.ToTroopString(),
                    CommandPopulation = Native.ArmyStats.CalculateTotalPopulation(instructionArmy),
                    CommandAttackPower = BattleSimulator.TotalAttackPower(instructionArmy),
                    CommandDefensePower = BattleSimulator.TotalDefensePower(instructionArmy),

                    SourceVillageId = instruction.SendFrom,
                    TargetVillageId = instruction.SendTo,

                    SourceVillageName = villagesById[instruction.SendFrom].Village.VillageName,
                    TargetVillageName = targetVillage.VillageName,

                    SourceVillageX = villagesById[instruction.SendFrom].Village.X.Value,
                    SourceVillageY = villagesById[instruction.SendFrom].Village.Y.Value,

                    TargetVillageX = targetVillage.X.Value,
                    TargetVillageY = targetVillage.Y.Value
                });
            }

            options = options.OrderBy(o => o.LaunchAt).ToList();

            return Ok(options);
        }
    }
}
