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

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Village")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class VillageController : BaseController
    {
        public VillageController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }
        
        [HttpGet(Name = "GetVillages")]
        public async Task<IActionResult> Get()
        {
            var villages = await Paginated(context.Village).FromWorld(CurrentWorldId).ToListAsync();
            return Ok(villages.Select(VillageConvert.ModelToJson));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Village.FromWorld(CurrentWorldId).CountAsync());
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
                    from village in context.Village.FromWorld(CurrentWorldId)
                    join player in context.Player on village.PlayerId.Value equals player.PlayerId
                    where village.VillageId == id
                    select player
                ).FirstOrDefaultAsync()
            );

            if (owner != null)
                return Ok(PlayerConvert.ModelToJson(owner));
            else
                return NotFound();
        }

        [HttpGet("{villageId}/army", Name = "GetKnownArmy")]
        public async Task<IActionResult> GetVillageArmy(long villageId, int? morale)
        {
            var village = await Profile("Find village", () => context.Village.Where(v => v.VillageId == villageId && v.WorldId == CurrentWorld.Id).FirstOrDefaultAsync());
            if (village == null)
                return NotFound();

            bool canRead = false;
            if (!village.PlayerId.HasValue)
            {
                //  Allowed to read for barbarian villages
                canRead = true;
            }
            else
            {
                var owningPlayer = await Profile("Get owning player", () => context.Player.Where(p => p.PlayerId == village.PlayerId).FirstOrDefaultAsync());
                if (!owningPlayer.TribeId.HasValue || owningPlayer.TribeId.Value != CurrentTribeId || CurrentUserIsAdmin)
                {
                    //  Allowed to read if:
                    //
                    // - the player has no tribe
                    // - or the village tribe is different from the player's tribe
                    // - or the current user is an admin
                    canRead = true;
                }
            }

            if (!canRead)
                return StatusCode(401);

            //  Check if player has uploaded report data recently
            var latestReportTransaction = await Profile("Find latest report transaction", () => (
                    from tx in context.Transaction
                        join report in context.Report on tx.TxId equals report.TxId
                    where tx.Uid == CurrentUser.Uid
                    orderby tx.OccurredAt descending
                    select tx
                ).FirstOrDefaultAsync()
            );

            if (latestReportTransaction == null || (DateTime.UtcNow - latestReportTransaction.OccurredAt) > TimeSpan.FromHours(24))
            {
                return StatusCode(423); // Status code "Locked"
            }

            //  Start getting village data
            var currentVillage = await Profile("Get current village", () => (
                    from cv in context.CurrentVillage
                                      .FromWorld(CurrentWorldId)
                                      .Include(v => v.ArmyOwned)
                                      .Include(v => v.ArmyStationed)
                                      .Include(v => v.ArmyTraveling)
                                      .Include(v => v.ArmyRecentLosses)
                                      .Include(v => v.CurrentBuilding)
                    where cv.VillageId == villageId
                    select cv
                ).FirstOrDefaultAsync()
            );

            var commandsToVillage = await Profile("Get commands to village", () => (
                    from command in context.Command
                                           .FromWorld(CurrentWorldId)
                                           .Include(c => c.Army)
                    where command.TargetVillageId == villageId
                    where !command.IsReturning
                    where command.LandsAt > CurrentServerTime
                    select command
                ).ToListAsync()
            );
            
            var jsonData = new JSON.VillageData();

            //  Return empty data if no data is available for the village
            if (currentVillage == null)
                return Ok(jsonData);

            Profile("Populate JSON data", () =>
            {
                if (currentVillage.ArmyOwned != null)
                {
                    jsonData.OwnedArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyOwned);
                    jsonData.OwnedArmySeenAt = currentVillage.ArmyOwned.LastUpdated;
                }

                if (currentVillage.ArmyRecentLosses != null)
                {
                    jsonData.RecentlyLostArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyRecentLosses);
                    jsonData.RecentlyLostArmySeenAt = currentVillage.ArmyRecentLosses.LastUpdated;
                }

                if (currentVillage.ArmyStationed != null)
                {
                    jsonData.StationedArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyStationed);
                    jsonData.StationedSeenAt = currentVillage.ArmyStationed.LastUpdated;
                }

                if (currentVillage.ArmyTraveling != null)
                {
                    jsonData.TravelingArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyTraveling);
                    jsonData.TravelingSeenAt = currentVillage.ArmyTraveling.LastUpdated;
                }

                jsonData.LastLoyalty = currentVillage.Loyalty;
                jsonData.LastLoyaltySeenAt = currentVillage.LoyaltyLastUpdated;

                if (currentVillage.Loyalty != null)
                {
                    var loyaltyCalculator = new LoyaltyCalculator();
                    jsonData.PossibleLoyalty = loyaltyCalculator.PossibleLoyalty(currentVillage.Loyalty.Value, CurrentServerTime - currentVillage.LoyaltyLastUpdated.Value);
                }

                jsonData.LastBuildings = BuildingConvert.CurrentBuildingToJson(currentVillage.CurrentBuilding);
                jsonData.LastBuildingsSeenAt = currentVillage.CurrentBuilding?.LastUpdated;

                if (currentVillage.CurrentBuilding != null)
                {
                    var constructionCalculator = new ConstructionCalculator();
                    jsonData.PossibleBuildings = constructionCalculator.CalculatePossibleBuildings(jsonData.LastBuildings, CurrentServerTime - currentVillage.CurrentBuilding.LastUpdated.Value);
                }

                if (currentVillage.ArmyStationed != null)
                {
                    var battleSimulator = new BattleSimulator();
                    short wallLevel = currentVillage.CurrentBuilding?.Wall ?? 20;
                    short hqLevel = currentVillage.CurrentBuilding?.Main ?? 20;

                    if (currentVillage.CurrentBuilding != null)
                        wallLevel += new ConstructionCalculator().CalculateLevelsInTimeSpan(BuildingType.Wall, hqLevel, wallLevel, CurrentServerTime - currentVillage.CurrentBuilding.LastUpdated.Value);

                    jsonData.NukesRequired = battleSimulator.EstimateRequiredNukes(jsonData.StationedArmy, wallLevel, morale ?? 100);
                }

                //  Might have CurrentArmy entries but they're just empty/null - not based on any report data
                if (jsonData.OwnedArmy != null && jsonData.OwnedArmySeenAt == null)
                    jsonData.OwnedArmy = null;

                if (jsonData.StationedArmy != null && jsonData.StationedSeenAt == null)
                    jsonData.StationedArmy = null;

                if (jsonData.TravelingArmy != null && jsonData.TravelingSeenAt == null)
                    jsonData.TravelingArmy = null;

                if (jsonData.RecentlyLostArmy != null && jsonData.RecentlyLostArmySeenAt == null)
                    jsonData.RecentlyLostArmy = null;



                //  Add recruitment estimations
                if (jsonData.StationedArmy != null)
                {
                    var timeSinceSeen = CurrentServerTime - jsonData.StationedSeenAt.Value;

                    //  No point in estimating troops if there's been 2 weeks since we saw stationed troops
                    if (timeSinceSeen.TotalDays < 14)
                    {
                        var calculator = new RecruitmentCalculator(2, jsonData.LastBuildings);
                        var existingPop = ArmyStats.CalculateTotalPopulation(jsonData.StationedArmy);
                        var availablePop = Math.Max(0, calculator.MaxPopulation - existingPop);

                        jsonData.PossibleRecruitedOffensiveArmy = calculator.CalculatePossibleOffenseRecruitment(timeSinceSeen);
                        jsonData.PossibleRecruitedDefensiveArmy = calculator.CalculatePossibleDefenseRecruitment(timeSinceSeen);
                    }
                }

                //  Add command summaries
                jsonData.DVs = new Dictionary<long, int>();
                jsonData.Fakes = new List<long>();
                jsonData.Nukes = new List<long>();

                jsonData.Players = commandsToVillage.Select(c => c.SourcePlayerId).Distinct().ToList();

                foreach (var command in commandsToVillage.Where(c => c.Army != null))
                {
                    var army = ArmyConvert.ArmyToJson(command.Army);
                    var offensivePop = ArmyStats.CalculateTotalPopulation(army.OfType(JSON.UnitBuild.Offensive));
                    var defensivePop = ArmyStats.CalculateTotalPopulation(army.OfType(JSON.UnitBuild.Defensive));

                    bool isFake = false;
                    bool isNuke = false;
                    if (!army.Values.Any(cnt => cnt > 1))
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

            var scaffoldCurrentVillages = await Profile("Get existing scaffold current villages", () => (
                    from cv in context.CurrentVillage.FromWorld(CurrentWorldId).IncludeCurrentVillageData()
                    where villageIds.Contains(cv.VillageId)
                    select cv
                ).ToListAsync()
            );

            var mappedScaffoldVillages = villageIds.ToDictionary(id => id, id => scaffoldCurrentVillages.SingleOrDefault(cv => cv.VillageId == id));
            var missingScaffoldVillageIds = mappedScaffoldVillages.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList();

            var missingVillageData = mappedScaffoldVillages.Values.Count(v => v == null) == 0
                ? new List<Scaffold.Village>()
                : await Profile("Get missing village data", () => (
                        from v in context.Village.FromWorld(CurrentWorldId)
                        where missingScaffoldVillageIds.Contains(v.VillageId)
                        select v
                    ).ToListAsync()
                );

            var villagePlayerIds = (await Profile("Get village player IDs", () => (
                    from v in context.Village.FromWorld(CurrentWorldId)
                    where villageIds.Contains(v.VillageId)
                    select new { v.PlayerId, v.VillageId }
                ).ToListAsync()
            )).ToDictionary(v => v.VillageId, v => v.PlayerId);

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

                    context.CurrentVillage.Add(newCurrentVillage);

                    mappedScaffoldVillages[missingVillageId] = newCurrentVillage;
                }
            });

            Profile("Generate scaffold armies", () =>
            {
                foreach (var armySetJson in currentArmySetJson.TroopData)
                {
                    var currentVillage = mappedScaffoldVillages[armySetJson.VillageId.Value];
                    var villagePlayerId = villagePlayerIds[currentVillage.VillageId];

                    if (!Configuration.Security.AllowUploadArmyForNonOwner
                            && villagePlayerId != CurrentUser.PlayerId)
                    {
                        context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                            JsonConvert.SerializeObject(currentArmySetJson),
                            $"Attempted to upload current army to village {villagePlayerId} but that village is not owned by the requestor"
                        ));
                    }

                    var fullArmy = armySetJson.AtHome + armySetJson.Traveling + armySetJson.Supporting;
                    currentVillage.ArmyOwned = ArmyConvert.JsonToArmy(fullArmy, currentVillage.ArmyOwned, context);
                    currentVillage.ArmyStationed = ArmyConvert.JsonToArmy(armySetJson.Stationed, currentVillage.ArmyStationed, context);
                    currentVillage.ArmyTraveling = ArmyConvert.JsonToArmy(armySetJson.Traveling, currentVillage.ArmyTraveling, context);
                    currentVillage.ArmyAtHome = ArmyConvert.JsonToArmy(armySetJson.AtHome, currentVillage.ArmyAtHome, context);
                    currentVillage.ArmySupporting = ArmyConvert.JsonToArmy(armySetJson.Supporting, currentVillage.ArmySupporting, context);

                    currentVillage.ArmyOwned.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyStationed.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyTraveling.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmyAtHome.LastUpdated = DateTime.UtcNow;
                    currentVillage.ArmySupporting.LastUpdated = DateTime.UtcNow;
                }
            });

            var currentPlayer = await EFUtil.GetOrCreateCurrentPlayer(context, CurrentUser.PlayerId, CurrentWorldId);
            currentPlayer.CurrentPossibleNobles = currentArmySetJson.PossibleNobles;

            var userUploadHistory = await EFUtil.GetOrCreateUserUploadHistory(context, CurrentUser.Uid);
            userUploadHistory.LastUploadedTroopsAt = DateTime.UtcNow;

            await Profile("Save changes", () => context.SaveChangesAsync());
            return Ok();
        }
    }
}
