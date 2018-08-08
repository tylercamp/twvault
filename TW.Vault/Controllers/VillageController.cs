using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Scaffold = TW.Vault.Scaffold_Model;
using JSON = TW.Vault.Model.JSON;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Calculations;
using TW.Vault.Features.Simulation;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Village")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class VillageController : ControllerBase
    {
        public VillageController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        // GET: api/Villages
        [HttpGet(Name = "GetVillages")]
        public async Task<IActionResult> Get()
        {
            return Ok(await Paginated(context.Village).ToListAsync());
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            return Ok(await context.Village.CountAsync());
        }

        // GET: api/Villages/5
        [HttpGet("{id}", Name = "GetVillage")]
        public Task<IActionResult> Get(int id)
        {
            return FindOr404<Scaffold.Village>(id);
        }

        [HttpGet("{id}/owner", Name = "GetOwner")]
        public async Task<IActionResult> GetOwner(int id)
        {
            var owner = await (
                from village in context.Village
                join player in context.Player on village.PlayerId.Value equals player.PlayerId
                where village.VillageId == id
                select player
            ).FirstOrDefaultAsync();

            if (owner != null)
                return Ok(owner);
            else
                return NotFound();
        }

        [HttpGet("{villageId}/army", Name = "GetKnownArmy")]
        public async Task<IActionResult> GetVillageArmy(long villageId)
        {
            var latestReportWithDefender = await (
                    from report in context.Report
                                          .Include(r => r.DefenderArmy)
                                          .Include(r => r.DefenderLossesArmy)
                    where report.DefenderArmyId.HasValue && report.DefenderVillageId == villageId
                    orderby report.OccuredAt descending
                    select report
                ).FirstOrDefaultAsync();

            var latestReportAsAttacker = await (
                    from report in context.Report
                                          .Include(r => r.AttackerArmy)
                                          .Include(r => r.AttackerLossesArmy)
                    where report.AttackerVillageId == villageId && (
                        report.AttackerArmy.Axe > 100 ||
                        report.AttackerArmy.Light > 100 ||
                        report.AttackerArmy.Spear > 100 ||
                        report.AttackerArmy.Sword > 100 ||
                        report.AttackerArmy.Heavy > 100
                    )
                    orderby report.OccuredAt descending
                    select report
                ).FirstOrDefaultAsync();

            var latestReportWithTravelingTroops = await (
                    from report in context.Report
                                          .Include(r => r.DefenderTravelingArmy)
                    where report.DefenderVillageId == villageId && report.DefenderTravelingArmyId.HasValue
                    orderby report.OccuredAt descending
                    select report
                ).FirstOrDefaultAsync();

            var latestReportWithBuildings = await (
                    from report in context.Report
                                          .Include(r => r.ReportBuilding)
                    where report.DefenderVillageId == villageId && report.ReportBuilding != null
                    orderby report.OccuredAt descending
                    select report
                ).FirstOrDefaultAsync();

            var jsonData = new JSON.VillageData();

            if (latestReportWithDefender != null)
            {
                var defenderTroops = ArmyConvert.ArmyToJson(latestReportWithDefender.DefenderArmy);
                var defenderLosses = ArmyConvert.ArmyToJson(latestReportWithDefender.DefenderLossesArmy);

                var finalCount = TroopCalculations.SubtractJson(defenderTroops, defenderLosses);
                jsonData.StationedArmy = finalCount;
                jsonData.StationedSeenAt = latestReportWithDefender.OccuredAt;
            }

            if (latestReportAsAttacker != null)
            {
                var attackerLosses = ArmyConvert.ArmyToJson(latestReportAsAttacker.AttackerLossesArmy);

                jsonData.RecentlyLostArmy = attackerLosses;
                jsonData.RecentlyLostArmySeenAt = latestReportAsAttacker.OccuredAt;
            }

            if (latestReportWithTravelingTroops != null)
            {
                var travelingTroops = ArmyConvert.ArmyToJson(latestReportWithTravelingTroops.DefenderTravelingArmy);
                jsonData.TravelingArmy = travelingTroops;
                jsonData.TravelingSeenAt = latestReportWithTravelingTroops.OccuredAt;
            }

            if (latestReportWithBuildings != null)
            {
                var buildings = BuildingConvert.ToJSON(latestReportWithBuildings.ReportBuilding);
                jsonData.LastBuildings = buildings;
                jsonData.LastBuildingsSeenAt = latestReportWithBuildings.OccuredAt;

                //  Manual correction for server time zone and en100 server time
                var now = DateTime.UtcNow + TimeSpan.FromHours(1);
                jsonData.PossibleBuildings = new ConstructionCalculator().CalculatePossibleBuildings(buildings, now - latestReportWithBuildings.OccuredAt);
            }

            return Ok(jsonData);
        }
    }
}
