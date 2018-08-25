using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class ReportConvert
    {
        public static void ToModel(this JSON.Report report, Scaffold.Report target, Scaffold.VaultContext context = null) => JsonToModel(report, target, context);
        public static JSON.Report ToJSON(this Scaffold.Report report) => ModelToJson(report);

        public static void JsonToModel(JSON.Report report, Scaffold.Report target, Scaffold.VaultContext context = null)
        {
            target.ReportId              = report.ReportId.Value;
            target.OccuredAt             = report.OccurredAt.Value;
            target.Morale                = report.Morale.Value;
            target.Luck                  = report.Luck.Value;
            target.Loyalty               = report.Loyalty;

            target.AttackerPlayerId      = report.AttackingPlayerId;
            target.AttackerVillageId     = report.AttackingVillageId.Value;

            target.AttackerArmy          = ArmyConvert.JsonToArmy(report.AttackingArmy, target.AttackerArmy, context);
            target.AttackerLossesArmy    = ArmyConvert.JsonToArmy(report.AttackingArmyLosses, target.AttackerLossesArmy, context);

            target.DefenderPlayerId      = report.DefendingPlayerId;
            target.DefenderVillageId     = report.DefendingVillageId.Value;

            target.DefenderArmy          = ArmyConvert.JsonToArmy(report.DefendingArmy, target.DefenderArmy, context);
            target.DefenderLossesArmy    = ArmyConvert.JsonToArmy(report.DefendingArmyLosses, target.DefenderLossesArmy, context);
            target.DefenderTravelingArmy = ArmyConvert.JsonToArmy(report.TravelingTroops, target.DefenderTravelingArmy, context);

            target.Building              = BuildingConvert.JsonToReportBuilding(report.BuildingLevels, target.Building, context);



            if (target.AttackerArmy != null)
                target.AttackerArmy.WorldId = target.World?.Id ?? target.WorldId;

            if (target.AttackerLossesArmy != null)
                target.AttackerLossesArmy.WorldId = target.World?.Id ?? target.WorldId;

            if (target.DefenderArmy != null)
                target.DefenderArmy.WorldId = target.World?.Id ?? target.WorldId;

            if (target.DefenderLossesArmy != null)
                target.DefenderLossesArmy.WorldId = target.World?.Id ?? target.WorldId;

            if (target.DefenderTravelingArmy != null)
                target.DefenderTravelingArmy.WorldId = target.World?.Id ?? target.WorldId;

            if (target.Building != null)
                target.Building.WorldId = target.World?.Id ?? target.WorldId;
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
