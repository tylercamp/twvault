using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;

namespace TW.Vault.Model.Native
{
    public static class ArmyStats
    {
        public static readonly List<JSON.TroopType> TroopTypes = Enum.GetValues(typeof(JSON.TroopType)).Cast<JSON.TroopType>().ToList();

        public static readonly Dictionary<JSON.TroopType, TimeSpan> BaseRecruitTime = new Dictionary<JSON.TroopType, TimeSpan>
        {
            { JSON.TroopType.Spear,     TimeSpan.FromSeconds(1020) },
            { JSON.TroopType.Sword,     TimeSpan.FromSeconds(1500) },
            { JSON.TroopType.Axe,       TimeSpan.FromSeconds(1320) },
            { JSON.TroopType.Archer,    TimeSpan.FromSeconds(1800) },
            { JSON.TroopType.Spy,       TimeSpan.FromSeconds(900) },
            { JSON.TroopType.Light,     TimeSpan.FromSeconds(1800) },
            { JSON.TroopType.Marcher,   TimeSpan.FromSeconds(3600) },
            { JSON.TroopType.Heavy,     TimeSpan.FromSeconds(4800) },
            { JSON.TroopType.Ram,       TimeSpan.FromSeconds(4800) },
            { JSON.TroopType.Catapult,  TimeSpan.FromSeconds(7200) },
            { JSON.TroopType.Knight,    TimeSpan.FromSeconds(21600) },
            { JSON.TroopType.Snob,      TimeSpan.FromSeconds(18000) },
            { JSON.TroopType.Militia,   TimeSpan.FromSeconds(0) }
        };

        public static readonly Dictionary<JSON.TroopType, int> Population = new Dictionary<JSON.TroopType, int>
        {
            { JSON.TroopType.Spear, 1 },
            { JSON.TroopType.Sword, 1 },
            { JSON.TroopType.Axe, 1 },
            { JSON.TroopType.Archer, 1 },
            { JSON.TroopType.Spy, 2 },
            { JSON.TroopType.Light, 4 },
            { JSON.TroopType.Marcher, 5 },
            { JSON.TroopType.Heavy, 6 },
            { JSON.TroopType.Ram, 5 },
            { JSON.TroopType.Catapult, 8 },
            { JSON.TroopType.Knight, 10 },
            { JSON.TroopType.Snob, 100 },
            { JSON.TroopType.Militia, 0 },
        };

        public static readonly Dictionary<JSON.TroopType, int> AttackPower = new Dictionary<JSON.TroopType, int>
        {
            { JSON.TroopType.Spear, 10 },
            { JSON.TroopType.Sword, 25 },
            { JSON.TroopType.Axe, 40 },
            { JSON.TroopType.Archer, 15 },
            { JSON.TroopType.Spy, 0 },
            { JSON.TroopType.Light, 130 },
            { JSON.TroopType.Marcher, 120 },
            { JSON.TroopType.Heavy, 150 },
            { JSON.TroopType.Ram, 2 },
            { JSON.TroopType.Catapult, 100 },
            { JSON.TroopType.Knight, 150 },
            { JSON.TroopType.Snob, 30 },
            { JSON.TroopType.Militia, 0 },
        };

        public static readonly Dictionary<JSON.TroopType, JSON.UnitPower> DefensePower = new Dictionary<JSON.TroopType, JSON.UnitPower>
        {
            { JSON.TroopType.Spear, new JSON.UnitPower { Infantry = 15, Cavalry = 45, Archer = 20 } },
            { JSON.TroopType.Sword, new JSON.UnitPower { Infantry = 50, Cavalry = 25, Archer = 40 } },
            { JSON.TroopType.Axe, new JSON.UnitPower { Infantry = 10, Cavalry = 5, Archer = 10 } },
            { JSON.TroopType.Archer, new JSON.UnitPower { Infantry = 50, Cavalry = 40, Archer = 5 } },
            { JSON.TroopType.Spy, new JSON.UnitPower { Infantry = 2, Cavalry = 1, Archer = 2 } },
            { JSON.TroopType.Light, new JSON.UnitPower { Infantry = 30, Cavalry = 40, Archer = 30 } },
            { JSON.TroopType.Marcher, new JSON.UnitPower { Infantry = 40, Cavalry = 30, Archer = 50 } },
            { JSON.TroopType.Heavy, new JSON.UnitPower { Infantry = 200, Cavalry = 80, Archer = 180 } },
            { JSON.TroopType.Ram, new JSON.UnitPower { Infantry = 20, Cavalry = 50, Archer = 20 } },
            { JSON.TroopType.Catapult, new JSON.UnitPower { Infantry = 100, Cavalry = 50, Archer = 100 } },
            { JSON.TroopType.Knight, new JSON.UnitPower { Infantry = 250, Cavalry = 400, Archer = 150 } },
            { JSON.TroopType.Snob, new JSON.UnitPower { Infantry = 100, Cavalry = 50, Archer = 100 } },
            { JSON.TroopType.Militia, new JSON.UnitPower { Infantry = 15, Cavalry = 45, Archer = 25 } }
        };

        public static readonly Dictionary<JSON.TroopType, JSON.UnitType> UnitType = new Dictionary<JSON.TroopType, JSON.UnitType>
        {
            { JSON.TroopType.Spear, JSON.UnitType.Infantry },
            { JSON.TroopType.Sword, JSON.UnitType.Infantry },
            { JSON.TroopType.Axe, JSON.UnitType.Infantry },
            { JSON.TroopType.Archer, JSON.UnitType.Archer },
            { JSON.TroopType.Spy, JSON.UnitType.Cavalry },
            { JSON.TroopType.Light, JSON.UnitType.Cavalry },
            { JSON.TroopType.Marcher, JSON.UnitType.Archer },
            { JSON.TroopType.Heavy, JSON.UnitType.Cavalry },
            { JSON.TroopType.Ram, JSON.UnitType.Infantry },
            { JSON.TroopType.Catapult, JSON.UnitType.Infantry },
            { JSON.TroopType.Knight, JSON.UnitType.Infantry },
            { JSON.TroopType.Snob, JSON.UnitType.Infantry },
            { JSON.TroopType.Militia, JSON.UnitType.Infantry },
        };

        public static readonly Dictionary<JSON.TroopType, JSON.UnitBuild> UnitBuild = new Dictionary<JSON.TroopType, JSON.UnitBuild>
        {
            { JSON.TroopType.Spear, JSON.UnitBuild.Defensive },
            { JSON.TroopType.Sword, JSON.UnitBuild.Defensive },
            { JSON.TroopType.Axe, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Archer, JSON.UnitBuild.Defensive },
            { JSON.TroopType.Spy, JSON.UnitBuild.Neutral },
            { JSON.TroopType.Light, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Marcher, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Heavy, JSON.UnitBuild.Defensive },
            { JSON.TroopType.Ram, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Catapult, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Knight, JSON.UnitBuild.Neutral },
            { JSON.TroopType.Snob, JSON.UnitBuild.Offensive },
            { JSON.TroopType.Militia, JSON.UnitBuild.Neutral }
        };

        public static readonly Dictionary<JSON.TroopType, BuildingType> UnitSource = new Dictionary<JSON.TroopType, BuildingType>
        {
            { JSON.TroopType.Spear, BuildingType.Barracks },
            { JSON.TroopType.Sword, BuildingType.Barracks },
            { JSON.TroopType.Axe, BuildingType.Barracks },
            { JSON.TroopType.Archer, BuildingType.Barracks },
            { JSON.TroopType.Spy, BuildingType.Stable },
            { JSON.TroopType.Light, BuildingType.Stable },
            { JSON.TroopType.Marcher, BuildingType.Stable },
            { JSON.TroopType.Heavy, BuildingType.Stable },
            { JSON.TroopType.Ram, BuildingType.Garage },
            { JSON.TroopType.Catapult, BuildingType.Garage },
            { JSON.TroopType.Snob, BuildingType.Snob }
        };

        public static readonly Dictionary<JSON.TroopType, float> TravelSpeed = new Dictionary<JSON.TroopType, float>
        {
            { JSON.TroopType.Spear, 18 },
            { JSON.TroopType.Sword, 22 },
            { JSON.TroopType.Axe, 18 },
            { JSON.TroopType.Archer, 18 },
            { JSON.TroopType.Spy, 9 },
            { JSON.TroopType.Light, 10 },
            { JSON.TroopType.Marcher, 10 },
            { JSON.TroopType.Heavy, 11 },
            { JSON.TroopType.Ram, 30 },
            { JSON.TroopType.Catapult, 30 },
            { JSON.TroopType.Knight, 10 },
            { JSON.TroopType.Snob, 35 },
            { JSON.TroopType.Militia, 0 },
        };

        public static readonly Dictionary<int, float> WallDefenseBuff = new Dictionary<int, float>
        {
            { 00, 1.00f },
            { 01, 1.04f },
            { 02, 1.08f },
            { 03, 1.12f },
            { 04, 1.16f },
            { 05, 1.20f },
            { 06, 1.24f },
            { 07, 1.29f },
            { 08, 1.34f },
            { 09, 1.39f },
            { 10, 1.44f },
            { 11, 1.49f },
            { 12, 1.55f },
            { 13, 1.60f },
            { 14, 1.66f },
            { 15, 1.7246f },
            { 16, 1.7885f },
            { 17, 1.8545f },
            { 18, 1.923f },
            { 19, 1.99425f },
            { 20, 2.068f }
        };

        public static readonly Dictionary<int, int> WallBonusDefense = new Dictionary<int, int>
        {
            { 00, 20 + 50*00 },
            { 01, 20 + 50*01 },
            { 02, 20 + 50*02 },
            { 03, 20 + 50*03 },
            { 04, 20 + 50*04 },
            { 05, 20 + 50*05 },
            { 06, 20 + 50*06 },
            { 07, 20 + 50*07 },
            { 08, 20 + 50*08 },
            { 09, 20 + 50*09 },
            { 10, 20 + 50*10 },
            { 11, 20 + 50*11 },
            { 12, 20 + 50*12 },
            { 13, 20 + 50*13 },
            { 14, 20 + 50*14 },
            { 15, 20 + 50*15 },
            { 16, 20 + 50*16 },
            { 17, 20 + 50*17 },
            { 18, 20 + 50*18 },
            { 19, 20 + 50*19 },
            { 20, 20 + 50*20 },
        };

        public static JSON.TroopType[] OffensiveTroopTypes { get; } = new[] { JSON.TroopType.Axe, JSON.TroopType.Light, JSON.TroopType.Marcher, JSON.TroopType.Heavy, JSON.TroopType.Ram, JSON.TroopType.Catapult };
        public static JSON.TroopType[] DefensiveTroopTypes { get; } = new[] { JSON.TroopType.Spear, JSON.TroopType.Sword, JSON.TroopType.Archer, JSON.TroopType.Heavy };

        public static int CalculateTotalPopulation(JSON.Army armyCounts, params JSON.TroopType[] troopTypes)
        {
            if (armyCounts == null)
                return 0;

            int totalPopulation = 0;
            foreach (var kvp in armyCounts.Where(kvp => troopTypes.Length == 0 || troopTypes.Contains(kvp.Key)))
                totalPopulation += kvp.Value * Population[kvp.Key];

            return totalPopulation;
        }

        public static int FullNukePopulation => 18000;
        public static int FullDVPopulation => 20000;

        public static int FullNukeOffensivePower = 450000;
        public static int FullDVDefensivePower = 1850000;

        public static bool IsOffensive(JSON.Army army) => army[JSON.TroopType.Axe] > 200 || army[JSON.TroopType.Light] > 100 || army[JSON.TroopType.Marcher] > 100;
        public static bool IsDefensive(JSON.Army army) => army[JSON.TroopType.Spear] > 500 || army[JSON.TroopType.Sword] > 500;
        public static bool IsNuke(JSON.Army army) => CalculateTotalPopulation(army, OffensiveTroopTypes) >= FullNukePopulation;
        public static bool IsNuke(JSON.Army army, double nukePercent) => CalculateTotalPopulation(army, OffensiveTroopTypes) >= FullNukePopulation * nukePercent;
        public static bool IsFang(JSON.Army army) => army[JSON.TroopType.Catapult] >= 50 && CalculateTotalPopulation(army) < 8000;
        public static bool IsFake(JSON.Army army) => CalculateTotalPopulation(army) < 350;

        /*
            { JSON.TroopType.Spear,  },
            { JSON.TroopType.Sword,  },
            { JSON.TroopType.Axe,  },
            { JSON.TroopType.Archer,  },
            { JSON.TroopType.Spy,  },
            { JSON.TroopType.Light,  },
            { JSON.TroopType.Marcher,  },
            { JSON.TroopType.Heavy,  },
            { JSON.TroopType.Ram,  },
            { JSON.TroopType.Catapult,  },
            { JSON.TroopType.Knight,  },
            { JSON.TroopType.Snob,  },
            { JSON.TroopType.Militia,  },
        */
    }
}
