using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Features.Simulation;
using TW.Vault.Lib.Model;
using TW.Vault.Lib.Model.Native;
using JSON = TW.Vault.Lib.Model.JSON;

namespace TW.Vault.Lib.Features.CommandClassification
{
    public static class Utils
    {
        public static int NukeAttackPower    =  400000;
        public static int StackDefensePower  = 1700000;
        public static int FullArmyPopulation =   19000;


        public static bool IsNuke(JSON.Army army)
        {
            return
                BattleSimulator.TotalAttackPower(army) > 4e5 &&
                ArmyStats.CalculateTotalPopulation(army.OfType(JSON.UnitBuild.Offensive)) > 10000 &&
                ArmyStats.CalculateTotalPopulation(army) > FullArmyPopulation;
        }

        public static bool IsOffensive(JSON.Army army)
        {
            return army.GetValueOrDefault(JSON.TroopType.Axe, 0) > 500 || army.GetValueOrDefault(JSON.TroopType.Light, 0) > 250;
        }

        public static bool IsStacked(JSON.Army army)
        {
            return BattleSimulator.TotalDefensePower(army) >= StackDefensePower;
        }
    }
}
