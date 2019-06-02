using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;

namespace TW.Vault.Model
{
    public static class Extensions
    {
        public static bool IsEmpty(this Scaffold.CurrentArmy army)
        {
            return army == null || army.LastUpdated == null ||
                (
                    (army.Spear == null || army.Spear.Value == 0) &&
                    (army.Sword == null || army.Sword.Value == 0) &&
                    (army.Axe == null || army.Axe.Value == 0) &&
                    (army.Archer == null || army.Archer.Value == 0) &&
                    (army.Spy == null || army.Spy.Value == 0) &&
                    (army.Light == null || army.Light.Value == 0) &&
                    (army.Marcher == null || army.Marcher.Value == 0) &&
                    (army.Heavy == null || army.Heavy.Value == 0) &&
                    (army.Ram == null || army.Ram.Value == 0) &&
                    (army.Catapult == null || army.Catapult.Value == 0) &&
                    (army.Knight == null || army.Knight.Value == 0) &&
                    (army.Snob == null || army.Snob.Value == 0) &&
                    (army.Militia == null || army.Militia.Value == 0)
                );
        }

        public static Scaffold.CurrentArmy OfType(this Scaffold.CurrentArmy army, JSON.UnitBuild build)
        {
            var result = new Scaffold.CurrentArmy();
            switch (build)
            {
                case JSON.UnitBuild.Offensive:
                    result.Axe = army.Axe;
                    result.Light = army.Light;
                    result.Marcher = army.Marcher;
                    result.Ram = army.Ram;
                    result.Catapult = army.Catapult;
                    result.Snob = army.Snob;
                    break;

                case JSON.UnitBuild.Defensive:
                    result.Spear = army.Spear;
                    result.Sword = army.Sword;
                    result.Archer = army.Archer;
                    result.Heavy = army.Heavy;
                    result.Militia = army.Militia;
                    break;

                case JSON.UnitBuild.Neutral:
                    result.Spy = army.Spy;
                    result.Knight = army.Knight;
                    break;
            }
            return result;
        }

        public static JSON.Army OfType(this JSON.Army army, JSON.UnitBuild build)
        {
            if (army == null)
                return new JSON.Army();

            var result = new JSON.Army();
            foreach (var type in army.Keys)
            {
                if (Native.ArmyStats.UnitBuild[type] == build)
                    result.Add(type, army[type]);
            }
            return result;
        }
    }
}
