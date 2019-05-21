using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using JSON = TW.Vault.Model.JSON;
using TW.Vault.Model.Convert;
using TW.Vault.Features.Simulation;
using Newtonsoft.Json;
using TW.Vault.Model.Native;
using TW.Vault.Model;
using TW.Vault.Model.Validation;
using System.Net;
using TW.Vault.Model.JSON;
using Microsoft.Extensions.DependencyInjection;
using TW.Vault.Scaffold;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Village")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class VillageController : BaseController
    {
        public VillageController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet(Name = "GetVillages")]
        public async Task<IActionResult> Get()
        {
            var villages = await Paginated(CurrentSets.Village).ToListAsync();
            return Ok(villages.Select(VillageConvert.ModelToJson));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await CurrentSets.Village.CountAsync());
        }
        
        [HttpGet("{id}", Name = "GetVillage")]
        public Task<IActionResult> Get(int id)
        {
            return SelectOr404<Scaffold.Village>(q => q.FromWorld(CurrentWorldId), VillageConvert.ModelToJson);
        }

        [HttpGet("{id}/owner", Name = "GetOwner")]
        public async Task<IActionResult> GetOwner(int id)
        {
            var owner = await Profile("Get village owner", () => (
                    from village in CurrentSets.Village
                    join player in CurrentSets.Player on village.PlayerId.Value equals player.PlayerId
                    where village.VillageId == id
                    select player
                ).FirstOrDefaultAsync()
            );

            if (owner != null)
                return Ok(PlayerConvert.ModelToJson(owner));
            else
                return NotFound();
        }

        //  I pity whoever tries to follow this whole function without guidance...
        [HttpGet("{villageId}/army", Name = "GetKnownArmy")]
        public async Task<IActionResult> GetVillageArmy(long villageId, int? morale)
        {
            if (!await CanReadVillage(villageId))
                return StatusCode(401);

            PreloadWorldData();

            var uploadHistory = await Profile("Get user upload history", () =>
                context.UserUploadHistory.Where(h => h.Uid == CurrentUserId).FirstOrDefaultAsync()
            );

            var validationInfo = UploadRestrictionsValidate.ValidateInfo.FromMapRestrictions(CurrentUser, uploadHistory);
            List<String> needsUpdateReasons = UploadRestrictionsValidate.GetNeedsUpdateReasons(DateTime.UtcNow, validationInfo);

            if (needsUpdateReasons != null && needsUpdateReasons.Any())
            {
                return StatusCode(423, needsUpdateReasons.Select(r => Translate(r)).ToList()); // Status code "Locked"
            }

            //  Start getting village data

            var (currentVillage, commandsToVillage, latestConquerTimestamp) = await ManyTasks.Run(
                Profile("Get current village", () => (
                    from cv in CurrentSets.CurrentVillage
                                        .Include(v => v.ArmyAtHome)
                                        .Include(v => v.ArmyOwned)
                                        .Include(v => v.ArmyStationed)
                                        .Include(v => v.ArmyTraveling)
                                        .Include(v => v.ArmyRecentLosses)
                                        .Include(v => v.CurrentBuilding)
                    where cv.VillageId == villageId
                    select cv
                ).FirstOrDefaultAsync()),

                Profile("Get commands to village", () => (
                    from command in CurrentSets.Command
                                               .Include(c => c.Army)
                    where command.TargetVillageId == villageId
                    where !command.IsReturning
                    where command.LandsAt > CurrentServerTime
                    select command
                ).ToListAsync()),

                Profile("Get latest conquer", () => (
                    from conquer in CurrentSets.Conquer
                    where conquer.VillageId == villageId
                    orderby conquer.UnixTimestamp descending
                    select conquer.UnixTimestamp
                ).FirstOrDefaultAsync())
            );
            
            
            var jsonData = new JSON.VillageData();

            //  Return empty data if no data is available for the village
            if (currentVillage == null)
                return Ok(jsonData);

            Profile("Populate JSON data", () =>
            {
                if (currentVillage.ArmyOwned?.LastUpdated != null)
                {
                    jsonData.OwnedArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyOwned);
                    jsonData.OwnedArmySeenAt = currentVillage.ArmyOwned.LastUpdated;
                }

                if (currentVillage.ArmyRecentLosses?.LastUpdated != null)
                {
                    jsonData.RecentlyLostArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyRecentLosses);
                    jsonData.RecentlyLostArmySeenAt = currentVillage.ArmyRecentLosses.LastUpdated;
                }

                if (currentVillage.ArmyStationed?.LastUpdated != null)
                {
                    jsonData.StationedArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyStationed);
                    jsonData.StationedSeenAt = currentVillage.ArmyStationed.LastUpdated;
                }

                if (currentVillage.ArmyTraveling?.LastUpdated != null)
                {
                    jsonData.TravelingArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyTraveling);
                    jsonData.TravelingSeenAt = currentVillage.ArmyTraveling.LastUpdated;
                }

                if (currentVillage.ArmyAtHome?.LastUpdated != null)
                {
                    jsonData.AtHomeArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyAtHome);
                    jsonData.AtHomeSeenAt = currentVillage.ArmyAtHome.LastUpdated;
                }

                jsonData.LastLoyalty = currentVillage.Loyalty;
                jsonData.LastLoyaltySeenAt = currentVillage.LoyaltyLastUpdated;

                if (currentVillage.Loyalty != null)
                {
                    var loyaltyCalculator = new LoyaltyCalculator(CurrentWorldSettings.GameSpeed);
                    jsonData.PossibleLoyalty = loyaltyCalculator.PossibleLoyalty(currentVillage.Loyalty.Value, CurrentServerTime - currentVillage.LoyaltyLastUpdated.Value);
                }

                jsonData.LastBuildings = BuildingConvert.CurrentBuildingToJson(currentVillage.CurrentBuilding);
                jsonData.LastBuildingsSeenAt = currentVillage.CurrentBuilding?.LastUpdated;

                if (currentVillage.CurrentBuilding?.LastUpdated != null)
                {
                    var constructionCalculator = new ConstructionCalculator();
                    jsonData.PossibleBuildings = constructionCalculator.CalculatePossibleBuildings(jsonData.LastBuildings, CurrentServerTime - currentVillage.CurrentBuilding.LastUpdated.Value);
                }

                if (currentVillage.ArmyStationed?.LastUpdated != null)
                {
                    var battleSimulator = new BattleSimulator();
                    short wallLevel = currentVillage.CurrentBuilding?.Wall ?? 20;
                    short hqLevel = currentVillage.CurrentBuilding?.Main ?? 20;

                    if (currentVillage.CurrentBuilding != null)
                        wallLevel += new ConstructionCalculator().CalculateLevelsInTimeSpan(BuildingType.Wall, hqLevel, wallLevel, CurrentServerTime - currentVillage.CurrentBuilding.LastUpdated.Value);

                    var nukeEstimation = battleSimulator.EstimateRequiredNukes(jsonData.StationedArmy, wallLevel, CurrentWorldSettings.ArchersEnabled, morale ?? 100);

                    jsonData.NukesRequired = nukeEstimation.NukesRequired;
                    jsonData.LastNukeLossPercent = (int)(nukeEstimation.LastNukeLossesPercent);
                }

                //  Might have CurrentArmy entries but they're just empty/null - not based on any report data
                if (jsonData.OwnedArmy != null && (jsonData.OwnedArmySeenAt == null || jsonData.OwnedArmy.Count == 0))
                {
                    jsonData.OwnedArmy = null;
                    jsonData.OwnedArmySeenAt = null;
                }

                if (jsonData.StationedArmy != null && (jsonData.StationedSeenAt == null || jsonData.StationedArmy.Count == 0))
                {
                    jsonData.StationedArmy = null;
                    jsonData.StationedSeenAt = null;
                }

                if (jsonData.TravelingArmy != null && (jsonData.TravelingSeenAt == null || jsonData.TravelingArmy.Count == 0))
                {
                    jsonData.TravelingArmy = null;
                    jsonData.TravelingSeenAt = null;
                }

                if (jsonData.RecentlyLostArmy != null && (jsonData.RecentlyLostArmySeenAt == null || jsonData.RecentlyLostArmy.Count == 0))
                {
                    jsonData.RecentlyLostArmy = null;
                    jsonData.RecentlyLostArmySeenAt = null;
                }


                var armyCalculator = new RecruitmentCalculator(2, jsonData.LastBuildings);
                DateTime? localArmyLastSeenAt = null;
                int? availableArmyPopulation = null;

                if (jsonData.StationedArmy != null)
                {
                    localArmyLastSeenAt = jsonData.StationedSeenAt.Value;
                    var existingPop = ArmyStats.CalculateTotalPopulation(jsonData.StationedArmy);
                    availableArmyPopulation = Math.Max(0, armyCalculator.MaxPopulation - existingPop);
                }

                var conquerTime = DateTimeOffset.FromUnixTimeMilliseconds(latestConquerTimestamp).UtcDateTime;

                bool useConquer = false;
                if (localArmyLastSeenAt == null)
                    useConquer = true;
                else
                    useConquer = conquerTime > localArmyLastSeenAt.Value;

                if (useConquer)
                {
                    localArmyLastSeenAt = conquerTime;
                    availableArmyPopulation = armyCalculator.MaxPopulation;
                }

                //  Add recruitment estimations
                if (localArmyLastSeenAt != null)
                {
                    var timeSinceSeen = CurrentServerTime - localArmyLastSeenAt.Value;
                    armyCalculator.MaxPopulation = availableArmyPopulation.Value;

                    //  No point in estimating troops if there's been 2 weeks since we saw stationed troops
                    if (timeSinceSeen.TotalDays < 14)
                    {
                        jsonData.PossibleRecruitedOffensiveArmy = armyCalculator.CalculatePossibleOffenseRecruitment(timeSinceSeen);
                        jsonData.PossibleRecruitedDefensiveArmy = armyCalculator.CalculatePossibleDefenseRecruitment(timeSinceSeen);
                    }
                }

                //  Add command summaries
                jsonData.DVs = new Dictionary<long, int>();
                jsonData.Fakes = new List<long>();
                jsonData.Nukes = new List<long>();
                jsonData.Nobles = new List<long>();

                jsonData.Players = commandsToVillage.Select(c => c.SourcePlayerId).Distinct().ToList();

                foreach (var command in commandsToVillage.Where(c => c.Army != null))
                {
                    var army = ArmyConvert.ArmyToJson(command.Army);
                    var offensivePop = ArmyStats.CalculateTotalPopulation(army.OfType(JSON.UnitBuild.Offensive));
                    var defensivePop = ArmyStats.CalculateTotalPopulation(army.OfType(JSON.UnitBuild.Defensive));
                    var isNoble = command.Army.Snob > 0 && command.IsAttack;

                    bool isFake = false;
                    bool isNuke = false;
                    if (!army.Values.Any(cnt => cnt > 1) && !isNoble)
                    {
                        isFake = true;
                    }
                    else if (command.IsAttack && offensivePop > 10000)
                    {
                        isNuke = true;
                    }

                    if (isFake)
                        jsonData.Fakes.Add(command.CommandId);
                    else if (isNuke)
                        jsonData.Nukes.Add(command.CommandId);
                    else if (defensivePop > 3000 && !command.IsAttack)
                        jsonData.DVs.Add(command.CommandId, defensivePop);

                    if (isNoble)
                        jsonData.Nobles.Add(command.CommandId);
                }
            });

            return Ok(jsonData);
        }

        [HttpPost("army/current")]
        public async Task<IActionResult> PostCurrentArmy([FromBody]JSON.PlayerArmy currentArmySetJson)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (currentArmySetJson.TroopData.Count == 0)
                return Ok();

            var villageIds = currentArmySetJson.TroopData.Select(a => a.VillageId.Value).ToList();

            var (scaffoldCurrentVillages, villagesWithPlayerIds) = await ManyTasks.Run(
                Profile("Get existing scaffold current villages", () => (
                    from cv in CurrentSets.CurrentVillage
                                          .Include(v => v.ArmyOwned)
                                          .Include(v => v.ArmyAtHome)
                                          .Include(v => v.ArmyStationed)
                                          .Include(v => v.ArmySupporting)
                                          .Include(v => v.ArmyTraveling)
                    where villageIds.Contains(cv.VillageId)
                    select cv
                ).ToListAsync())
                ,
                Profile("Get village player IDs", () => (
                    from v in CurrentSets.Village
                    where villageIds.Contains(v.VillageId)
                    select new { v.PlayerId, v.VillageId }
                ).ToListAsync())
            );

            var villageIdsByPlayerId = villagesWithPlayerIds.ToDictionary(v => v.VillageId, v => v.PlayerId);

            var mappedScaffoldVillages = villageIds.ToDictionary(id => id, id => scaffoldCurrentVillages.SingleOrDefault(cv => cv.VillageId == id));
            var missingScaffoldVillageIds = mappedScaffoldVillages.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList();

            var missingVillageData = mappedScaffoldVillages.Values.Count(v => v == null) == 0
                ? new List<Scaffold.Village>()
                : await Profile("Get missing village data", () => (
                        from v in CurrentSets.Village
                        where missingScaffoldVillageIds.Contains(v.VillageId)
                        select v
                    ).ToListAsync()
                );

            var mappedMissingVillageData = missingVillageData.ToDictionary(vd => vd.VillageId, vd => vd);

            //  Get or make CurrentVillage

            Profile("Populating missing village data", () =>
            {
                foreach (var missingVillageId in missingScaffoldVillageIds)
                {
                    var village = mappedMissingVillageData[missingVillageId];
                    var newCurrentVillage = new Scaffold.CurrentVillage();
                    newCurrentVillage.VillageId = missingVillageId;
                    newCurrentVillage.WorldId = CurrentWorldId;
                    newCurrentVillage.AccessGroupId = CurrentAccessGroupId;

                    context.CurrentVillage.Add(newCurrentVillage);

                    mappedScaffoldVillages[missingVillageId] = newCurrentVillage;
                }
            });

            Profile("Generate scaffold armies", () =>
            {
                foreach (var armySetJson in currentArmySetJson.TroopData)
                {
                    var currentVillage = mappedScaffoldVillages[armySetJson.VillageId.Value];
                    var villagePlayerId = villageIdsByPlayerId[currentVillage.VillageId];

                    if (!Configuration.Security.AllowUploadArmyForNonOwner
                            && villagePlayerId != CurrentPlayerId)
                    {
                        context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                            JsonConvert.SerializeObject(currentArmySetJson),
                            $"Attempted to upload current army to village {villagePlayerId} but that village is not owned by the requestor"
                        ));
                    }

                    var fullArmy = armySetJson.AtHome + armySetJson.Traveling + armySetJson.Supporting;
                    currentVillage.ArmyOwned = ArmyConvert.JsonToArmy(fullArmy, CurrentWorldId, currentVillage.ArmyOwned, context, emptyIfNull: true);
                    currentVillage.ArmyStationed = ArmyConvert.JsonToArmy(armySetJson.Stationed, CurrentWorldId, currentVillage.ArmyStationed, context, emptyIfNull: true);
                    currentVillage.ArmyTraveling = ArmyConvert.JsonToArmy(armySetJson.Traveling, CurrentWorldId, currentVillage.ArmyTraveling, context, emptyIfNull: true);
                    currentVillage.ArmyAtHome = ArmyConvert.JsonToArmy(armySetJson.AtHome, CurrentWorldId, currentVillage.ArmyAtHome, context, emptyIfNull: true);
                    currentVillage.ArmySupporting = ArmyConvert.JsonToArmy(armySetJson.Supporting, CurrentWorldId, currentVillage.ArmySupporting, context, emptyIfNull: true);


                    currentVillage.ArmyOwned.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyStationed.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyTraveling.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyAtHome.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmySupporting.LastUpdated = DateTime.UtcNow;
                }
            });

            var currentPlayer = await EFUtil.GetOrCreateCurrentPlayer(context, CurrentPlayerId, CurrentWorldId, CurrentAccessGroupId);
            currentPlayer.CurrentPossibleNobles = currentArmySetJson.PossibleNobles;

            await Profile("Save changes", () => context.SaveChangesAsync());

            //  Run upload history update in separate query to prevent creating multiple history
            //  entries
            var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUserId);
            userUploadHistory.LastUploadedTroopsAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{villageId}/commands")]
        public async Task<IActionResult> GetCommandsRegardingVillage(long villageId)
        {
            if (!await CanReadVillage(villageId))
                return StatusCode(401);

            var uploadHistory = await context.UserUploadHistory.Where(u => u.Uid == CurrentUserId).FirstOrDefaultAsync();
            var validationInfo = UploadRestrictionsValidate.ValidateInfo.FromMapRestrictions(CurrentUser, uploadHistory);
            var needsUpdateReasons = UploadRestrictionsValidate.GetNeedsUpdateReasons(DateTime.UtcNow, validationInfo);

            if (needsUpdateReasons != null && needsUpdateReasons.Any())
            {
                return StatusCode(423, needsUpdateReasons.Select(r => Translate(r)).ToList()); // Status code "Locked"
            }

            var commandsFromVillage = await Profile("Get commands from village", () => (
                    from command in CurrentSets.Command
                                               .FromWorld(CurrentWorldId)
                                               .Include(c => c.Army)
                    where command.SourceVillageId == villageId
                    where command.ReturnsAt > CurrentServerTime
                    orderby command.ReturnsAt ascending
                    select command
                ).ToListAsync());

            foreach (var command in commandsFromVillage.Where(c => !c.IsReturning))
            {
                if (command.LandsAt <= CurrentServerTime)
                    command.IsReturning = true;
            }

            await context.SaveChangesAsync();
            

            var targetVillageIds = commandsFromVillage.Select(c => c.TargetVillageId).Distinct();
            var targetVillages = await Profile("Get other villages", () => (
                    from village in CurrentSets.Village
                    where targetVillageIds.Contains(village.VillageId)
                    select village
                ).ToListAsync());

            var targetVillagesById = targetVillages.ToDictionary(v => v.VillageId, v => v);

            var result = new JSON.VillageCommandSet();

            if (commandsFromVillage != null && commandsFromVillage.Count > 0)
            {
                result.CommandsFromVillage = new List<JSON.VillageCommand>();

                foreach (var command in commandsFromVillage)
                {
                    var commandData = new JSON.VillageCommand();
                    commandData.CommandId = command.CommandId;
                    commandData.LandsAt = command.LandsAt;
                    commandData.ReturnsAt = command.ReturnsAt.Value;
                    commandData.Army = ArmyConvert.ArmyToJson(command.Army);
                    commandData.IsReturning = command.IsReturning;
                    commandData.TroopType = command.TroopType;
                    commandData.OtherVillageId = command.TargetVillageId;

                    var otherVillage = targetVillagesById[command.TargetVillageId];
                    commandData.OtherVillageName = otherVillage.VillageName.UrlDecode();
                    commandData.OtherVillageCoords = $"{otherVillage.X}|{otherVillage.Y}";

                    result.CommandsFromVillage.Add(commandData);
                }
            }

            return Ok(result);
        }


        [HttpGet("{x}/{y}/{width}/{height}/tags")]
        public async Task<IActionResult> GetMapTags(int x, int y, int width, int height)
        {
            var uploadHistory = await context.UserUploadHistory.Where(u => u.Uid == CurrentUserId).FirstOrDefaultAsync();
            var validationInfo = UploadRestrictionsValidate.ValidateInfo.FromMapRestrictions(CurrentUser, uploadHistory);
            var needsUpdateReasons = UploadRestrictionsValidate.GetNeedsUpdateReasons(DateTime.UtcNow, validationInfo);

            if (needsUpdateReasons != null && needsUpdateReasons.Any())
            {
                return StatusCode(423, needsUpdateReasons.Select(r => Translate(r)).ToList()); // Status code "Locked"
            }

            var vaultTribes = await Profile("Get tribe IDs", () => (
                from user in CurrentSets.User
                join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                where player.TribeId != null && user.Enabled
                select player.TribeId.Value
            ).Distinct().ToListAsync());

            var enemyTribes = await CurrentSets.EnemyTribe.Select(et => et.EnemyTribeId).ToListAsync();

            var villageData = await Profile("Get village data", () => (
                from currentVillage in CurrentSets.CurrentVillage
                                                  .Include(cv => cv.ArmyOwned)
                                                  .Include(cv => cv.ArmyTraveling)
                                                  .Include(cv => cv.ArmyStationed)
                                                  .Include(cv => cv.CurrentBuilding)

                join village in CurrentSets.Village on currentVillage.VillageId equals village.VillageId
                join player in CurrentSets.Player on village.PlayerId equals player.PlayerId
                where CurrentUserIsAdmin || player.TribeId == null || !vaultTribes.Contains(player.TribeId.Value)

                where village.X >= x && village.Y >= y && village.X <= x + width && village.Y <= y + height
                select new { CurrentVillage = currentVillage, player.PlayerId, player.TribeId }
            ).ToListAsync());

            var ownVillageData = await Profile("Get own village data", () => (
                from village in CurrentSets.Village
                join currentVillage in CurrentSets.CurrentVillage on village.VillageId equals currentVillage.VillageId
                where village.PlayerId == CurrentPlayerId
                select new { Village = village, currentVillage.ArmyAtHome }
            ).ToListAsync());

            var validVillages = villageData.Where(vd => vd.PlayerId != CurrentPlayerId).ToList();
            var validVillageIds = villageData.Select(d => d.CurrentVillage.VillageId).ToList();

            var commandData = await Profile("Get command data", () => (
                from command in CurrentSets.Command.FromWorld(CurrentWorldId)
                where command.IsReturning && command.ReturnsAt > CurrentServerTime && command.ArmyId != null
                select new { command.SourceVillageId, command.Army }
            ).ToListAsync());

            var commandsByVillageId = validVillageIds.ToDictionary(id => id, id => new List<Scaffold.CommandArmy>());
            foreach (var command in commandData.Where(cmd => commandsByVillageId.ContainsKey(cmd.SourceVillageId)))
                commandsByVillageId[command.SourceVillageId].Add(command.Army);
            
            var result = new Dictionary<long, JSON.VillageTags>();

            var tribeIds = validVillages.Select(vv => vv.TribeId).Where(t => t != null).Select(t => t.Value).Distinct();
            var tribeNames = await Profile("Get tribe names", () => (
                from tribe in CurrentSets.Ally
                where tribeIds.Contains(tribe.TribeId)
                select new { tribe.Tag, tribe.TribeId }
            ).ToListAsync());

            var tribeNamesById = tribeIds.ToDictionary(tid => tid, tid => tribeNames.First(tn => tn.TribeId == tid).Tag.UrlDecode());

            Profile("Generate JSON tags", () =>
            {
                foreach (var data in validVillages)
                {
                    var village = data.CurrentVillage;
                    var tag = new JSON.VillageTags();
                    tag.TribeName = data.TribeId == null ? null : tribeNamesById[data.TribeId.Value];
                    tag.IsEnemyTribe = data.TribeId == null ? false : enemyTribes.Contains(data.TribeId.Value);

                    tag.WallLevel = data.CurrentVillage.CurrentBuilding?.Wall;
                    tag.WallLevelSeenAt = data.CurrentVillage.CurrentBuilding?.LastUpdated;

                    tag.WatchtowerLevel = data.CurrentVillage.CurrentBuilding?.Watchtower;
                    tag.WatchtowerSeenAt = data.CurrentVillage.CurrentBuilding?.LastUpdated;

                    tag.ReturningTroopsPopulation = commandsByVillageId[data.CurrentVillage.VillageId].Sum((army) => ArmyStats.CalculateTotalPopulation(ArmyConvert.ArmyToJson(army)));

                    if (village.ArmyStationed?.LastUpdated != null)
                    {
                        // 1 DV is approx. 1.7m total defense power
                        var stationed = village.ArmyStationed;
                        var defensePower = BattleSimulator.TotalDefensePower(ArmyConvert.ArmyToJson(stationed));
                        tag.IsStacked = defensePower > 1.7e6;
                        tag.StackDVs = defensePower / (float)1.7e6;
                        tag.StackSeenAt = village.ArmyStationed.LastUpdated;
                    }

                    var validArmies = new[] {
                        village.ArmyOwned,
                        village.ArmyTraveling,
                        village.ArmyStationed
                    }.Where(a => a?.LastUpdated != null && !ArmyConvert.ArmyToJson(a).IsEmpty()).ToList();

                    bool IsNuke(Scaffold.CurrentArmy army)
                    {
                        var jsonArmy = ArmyConvert.ArmyToJson(army);
                        if (BattleSimulator.TotalAttackPower(jsonArmy) < 3.5e5)
                            return false;

                        if (ArmyStats.CalculateTotalPopulation(jsonArmy, TroopType.Axe, TroopType.Light, TroopType.Marcher) < 4000)
                            return false;

                        //  Check HC nuke
                        if (army.Light < 100)
                        {
                            return ArmyStats.CalculateTotalPopulation(jsonArmy, TroopType.Axe, TroopType.Heavy, TroopType.Marcher) > 15000 && army.Axe > army.Heavy;
                        }
                        else
                        {
                            //  13k pop, ie 5k axe, 2k lc
                            return ArmyStats.CalculateTotalPopulation(jsonArmy, TroopType.Axe, TroopType.Light, TroopType.Marcher) > 13000;
                        }
                    }

                    var nukeArmy = (
                            from army in validArmies
                            where IsNuke(army)
                            orderby army.LastUpdated.Value descending
                            select army
                        ).FirstOrDefault();

                    var nobleArmy = (
                            from army in validArmies
                            where army.Snob > 0
                            orderby army.LastUpdated.Value descending
                            select army
                        ).FirstOrDefault();

                    if (nukeArmy != null)
                    {
                        tag.HasNuke = true;
                        tag.NukeSeenAt = nukeArmy.LastUpdated;
                    }

                    if (nobleArmy != null)
                    {
                        tag.HasNobles = true;
                        tag.NoblesSeenAt = nobleArmy.LastUpdated;
                    }

                    result.Add(village.VillageId, tag);
                }
            });

            return Ok(result);
        }

        [HttpGet("coords")]
        public async Task<IActionResult> QueryCoords(String player = null, String tribe = null, String k = null, String min = null, String max = null, String center = null)
        {
            player = player ?? "";
            tribe = tribe ?? "";
            k = k ?? "";

            IEnumerable<String> TrimAndFilter(IEnumerable<String> e) => e.Select(n => n.Trim()).Where(n => n.Length > 0).Select(MiscExtensions.UrlEncode);

            var players = TrimAndFilter(player.Split(',')).ToList();
            var tribes = TrimAndFilter(tribe.Split(',')).ToList();
            var continents = TrimAndFilter(k.Split(',')).ToList();

            var query = new Features.VillageSearch.Query
            {
                WorldId = CurrentWorldId,
                PlayerNames = players,
                TribeNamesOrTags = tribes,
                Continents = continents
            };

            if (min != null)
            {
                var minParts = min.Split('|');
                query.MinCoord = new Coordinate
                {
                    X = int.Parse(minParts[0]),
                    Y = int.Parse(minParts[1])
                };
            }

            if (max != null)
            {
                var maxParts = max.Split('|');
                query.MaxCoord = new Coordinate
                {
                    X = int.Parse(maxParts[0]),
                    Y = int.Parse(maxParts[1])
                };
            }

            if (center != null)
            {
                var centerParts = center.Split('|');
                query.CenterCoord = new Coordinate
                {
                    X = int.Parse(centerParts[0]),
                    Y = int.Parse(centerParts[1])
                };

                query.MaxDistance = float.Parse(centerParts[2]);
            }

            var coords = await Features.VillageSearch.ListCoords(context, query);

            return Ok(new { coords });
        }



        private async Task<bool> CanReadVillage(long villageId, Scaffold.Village queriedVillage = null)
        {
            var village = queriedVillage ?? await Profile("Find village", () => CurrentSets.Village.Where(v => v.VillageId == villageId).FirstOrDefaultAsync());
            if (village == null)
                return false;

            var registeredTribeIds = await Profile("Get registered tribe IDs", () => (
                    from user in CurrentSets.User
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    where player.TribeId != null
                    where user.Enabled
                    select player.TribeId.Value
                ).Distinct().ToListAsync());

            bool canRead = false;
            if (!village.PlayerId.HasValue)
            {
                //  Allowed to read for barbarian villages
                canRead = true;
            }
            else
            {
                var owningPlayer = await Profile("Get owning player", () => CurrentSets.Player.Where(p => p.PlayerId == village.PlayerId).FirstOrDefaultAsync());
                bool canReadFromTribe = true;
                if (owningPlayer.TribeId != null)
                {
                    if (Configuration.Security.RestrictAccessWithinTribes)
                        canReadFromTribe = owningPlayer.TribeId.Value != CurrentTribeId;
                    else
                        canReadFromTribe = !registeredTribeIds.Contains(owningPlayer.TribeId.Value);
                }

                if (owningPlayer.PlayerId == CurrentPlayerId || !owningPlayer.TribeId.HasValue || canReadFromTribe || CurrentUserIsAdmin)
                {
                    //  Allowed to read if:
                    //
                    // - the player has no tribe
                    // - or the village tribe is different from the player's tribe
                    // - or the current user is an admin
                    canRead = true;
                }
            }

            return canRead;
        }
    }
}
