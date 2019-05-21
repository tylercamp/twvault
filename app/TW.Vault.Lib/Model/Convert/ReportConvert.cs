using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class ReportConvert
    {
        public static void ToModel(this JSON.Report report, short worldId, Scaffold.Report target, Scaffold.VaultContext context = null) => JsonToModel(report, worldId, target, context);
        public static JSON.Report ToJSON(this Scaffold.Report report) => ModelToJson(report);

        public static void JsonToModel(JSON.Report report, short worldId, Scaffold.Report target, Scaffold.VaultContext context = null)
        {
            target.WorldId               = worldId;
            target.ReportId              = report.ReportId.Value;
            target.OccuredAt             = report.OccurredAt.Value;
            target.Morale                = report.Morale.Value;
            target.Luck                  = report.Luck.Value;
            target.Loyalty               = report.Loyalty;

            target.AttackerPlayerId      = report.AttackingPlayerId;
            target.AttackerVillageId     = report.AttackingVillageId.Value;

            target.AttackerArmy          = ArmyConvert.JsonToArmy(report.AttackingArmy, worldId, target.AttackerArmy, context);
            target.AttackerLossesArmy    = ArmyConvert.JsonToArmy(report.AttackingArmyLosses, worldId, target.AttackerLossesArmy, context, emptyIfNull: true);

            target.DefenderPlayerId      = report.DefendingPlayerId;
            target.DefenderVillageId     = report.DefendingVillageId.Value;

            target.DefenderArmy          = ArmyConvert.JsonToArmy(report.DefendingArmy, worldId, target.DefenderArmy, context);
            target.DefenderLossesArmy    = ArmyConvert.JsonToArmy(report.DefendingArmyLosses, worldId, target.DefenderLossesArmy, context);
            target.DefenderTravelingArmy = ArmyConvert.JsonToArmy(report.TravelingTroops, worldId, target.DefenderTravelingArmy, context);

            target.Building              = BuildingConvert.JsonToReportBuilding(report.BuildingLevels, worldId, target.Building, context);
        }

        public static JSON.Report ModelToJson(Scaffold.Report report)
        {
            var result = new JSON.Report();

            result.ReportId             = report.ReportId;
            result.OccurredAt           = report.OccuredAt;
            result.Morale               = report.Morale;
            result.Luck                 = report.Luck;

            result.AttackingPlayerId    = report.AttackerPlayerId;
            result.AttackingVillageId   = report.AttackerVillageId;

            result.AttackingArmy        = ArmyConvert.ArmyToJson(report.AttackerArmy);
            result.AttackingArmyLosses  = ArmyConvert.ArmyToJson(report.AttackerLossesArmy);

            result.DefendingPlayerId    = report.DefenderPlayerId;
            result.DefendingVillageId   = report.DefenderVillageId;

            result.DefendingArmy        = ArmyConvert.ArmyToJson(report.DefenderArmy);
            result.DefendingArmyLosses  = ArmyConvert.ArmyToJson(report.DefenderLossesArmy);
            result.TravelingTroops      = ArmyConvert.ArmyToJson(report.DefenderTravelingArmy);

            result.BuildingLevels       = BuildingConvert.ReportBuildingToJson(report.Building);

            return result;
        }
    }
}
