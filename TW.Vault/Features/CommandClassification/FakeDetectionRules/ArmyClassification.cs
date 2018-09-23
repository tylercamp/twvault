using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold;

namespace TW.Vault.Features.CommandClassification.FakeDetectionRules
{
    public static class ArmyClassification
    {
        static TimeSpan MaxArmyAge = TimeSpan.FromDays(3);

        public static bool IsOffensiveArmy(CurrentArmy army) =>
            army.Axe > 1000 || army.Light > 400;

        public static bool IsDefensiveArmy(CurrentArmy army) =>
            army.Spear > 2000 || army.Sword > 2000;

        public static bool IsOffensiveArmy(CommandArmy army) =>
            army.Axe > 1000 || army.Light > 400;

        public static bool IsDefensiveArmy(CommandArmy army) =>
            army.Spear > 2000 || army.Sword > 2000;




        public static bool IsRecentArmy(CurrentArmy army, DateTime currentTime) =>
            army.LastUpdated != null && (currentTime - army.LastUpdated.Value) < MaxArmyAge;



        public static bool IsOffensive(this CurrentArmy army) => IsOffensiveArmy(army);
        public static bool IsDefensive(this CurrentArmy army) => IsDefensiveArmy(army);
        public static bool IsOffensive(this CommandArmy army) => IsOffensiveArmy(army);
        public static bool IsDefensive(this CommandArmy army) => IsDefensiveArmy(army);



        public static bool IsRecent(this CurrentArmy army, DateTime currentTime) => IsRecentArmy(army, currentTime);
    }
}
