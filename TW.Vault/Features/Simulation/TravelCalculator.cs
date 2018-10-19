using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;
using JSON = TW.Vault.Model.JSON;
using Native = TW.Vault.Model.Native;

namespace TW.Vault.Features.Simulation
{
    public class TravelCalculator
    {
        float worldSpeed;
        float unitSpeed;

        //  TODO - Check that all references are pulling from world data
        public TravelCalculator(float worldSpeed, float unitSpeed)
        {
            this.worldSpeed = worldSpeed;
            this.unitSpeed = unitSpeed;
        }

        public TravelCalculator(decimal worldSpeed, decimal unitSpeed)
        {
            this.worldSpeed = (float)worldSpeed;
            this.unitSpeed = (float)unitSpeed;
        }

        public JSON.TroopType TravelTroopType(JSON.Army army)
        {
            JSON.TroopType slowestType = JSON.TroopType.Spy;

            foreach (var type in army.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key))
            {
                if (Native.ArmyStats.TravelSpeed[type] > Native.ArmyStats.TravelSpeed[slowestType])
                    slowestType = type;
            }

            return slowestType;
        }

        public TimeSpan ArmyFieldSpeed(JSON.Army army)
        {
            var slowestSpeed = Native.ArmyStats.TravelSpeed[TravelTroopType(army)];
            return TimeSpan.FromMinutes(slowestSpeed);
        }

        public TimeSpan CalculateTravelTime(JSON.TroopType unitType, int startx, int starty, int endx, int endy)
        {
            var distance = Math.Sqrt((endx - startx) * (endx - startx) + (endy - starty) * (endy - starty));
            var minutesPerField = Native.ArmyStats.TravelSpeed[unitType];

            var minutes = distance * minutesPerField / worldSpeed / unitSpeed;
            return TimeSpan.FromMinutes(minutes);
        }

        public TimeSpan CalculateTravelTime(JSON.TroopType unitType, JSON.Village startVillage, JSON.Village endVillage) =>
            CalculateTravelTime(unitType, startVillage.X.Value, startVillage.Y.Value, endVillage.X.Value, endVillage.Y.Value);

        public TimeSpan CalculateTravelTime(JSON.TroopType unitType, Scaffold.Village startVillage, Scaffold.Village endVillage) =>
            CalculateTravelTime(unitType, startVillage.X.Value, startVillage.Y.Value, endVillage.X.Value, endVillage.Y.Value);

        public JSON.TroopType EstimateTroopType(TimeSpan travelTime, int startx, int starty, int endx, int endy)
        {
            //  TODO - Pull this from world settings
            bool archersEnabled = false;

            JSON.TroopType? type = null;
            TimeSpan? bestTime = null;
            foreach (var troopType in Native.ArmyStats.TroopTypes)
            {
                if (Native.ArmyStats.TravelSpeed[troopType] <= 0)
                    continue;

                if (!archersEnabled && (troopType == JSON.TroopType.Archer || troopType == JSON.TroopType.Marcher))
                    continue;

                var troopTravelTime = CalculateTravelTime(troopType, startx, starty, endx, endy);
                if (troopTravelTime >= travelTime)
                {
                    if (bestTime == null || bestTime.Value > troopTravelTime || bestTime.Value == troopTravelTime)
                    {
                        type = troopType;
                        bestTime = troopTravelTime;
                    }
                }
            }
            return type ?? JSON.TroopType.Spy;
        }
        public JSON.TroopType EstimateTroopType(TimeSpan travelTime, JSON.Village startVillage, JSON.Village endVillage) =>
            EstimateTroopType(travelTime, startVillage.X.Value, startVillage.Y.Value, endVillage.X.Value, endVillage.Y.Value);

        public JSON.TroopType EstimateTroopType(TimeSpan travelTime, Scaffold.Village startVillage, Scaffold.Village endVillage) =>
            EstimateTroopType(travelTime, startVillage.X.Value, startVillage.Y.Value, endVillage.X.Value, endVillage.Y.Value);
    }
}
