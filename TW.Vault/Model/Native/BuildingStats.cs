using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Native
{
    public static class BuildingStats
    {
        public static readonly List<BuildingType> BuildingTypes = Enum.GetValues(typeof(BuildingType)).Cast<BuildingType>().ToList();

        public static Dictionary<BuildingType, float[]> RecruitmentSpeedFactors = new Dictionary<BuildingType, float[]>
        {
            { BuildingType.Barracks, new [] {
                0.63f, 0.59f, 0.56f,
                0.53f, 0.50f, 0.47f,
                0.44f, 0.42f, 0.39f,
                0.37f, 0.35f, 0.33f,
                0.31f, 0.29f, 0.28f,
                0.26f, 0.25f, 0.23f,
                0.22f, 0.21f, 0.20f,
                0.19f, 0.17f, 0.165f,
                0.16f
            } },

            { BuildingType.Stable, new [] {
                0.63f, 0.59f, 0.56f,
                0.53f, 0.50f, 0.47f,
                0.44f, 0.42f, 0.39f,
                0.37f, 0.35f, 0.33f,
                0.31f, 0.29f, 0.28f,
                0.26f, 0.25f, 0.23f,
                0.22f, 0.21f
            } },

            { BuildingType.Garage, new [] {
                0.63f, 0.59f, 0.56f,
                0.53f, 0.50f, 0.47f,
                0.44f, 0.42f, 0.39f,
                0.37f, 0.35f, 0.33f,
                0.31f, 0.29f, 0.28f
            } },

            { BuildingType.Snob, new [] {
                0.63f, 0.59f, 0.56f
            } }
        };

        public static Dictionary<BuildingType, TimeSpan> LevelOneBuildTimes = new Dictionary<BuildingType, TimeSpan>
        {
            { BuildingType.Barracks,    TimeSpan.FromSeconds(900) },
            { BuildingType.Church,      TimeSpan.FromSeconds(0) }, // left at 0 since churches disabled on en100
            { BuildingType.Farm,        TimeSpan.FromSeconds(600) },
            { BuildingType.Garage,      TimeSpan.FromSeconds(3000) },
            { BuildingType.Hide,        TimeSpan.FromSeconds(900) },
            { BuildingType.Iron,        TimeSpan.FromSeconds(540) },
            { BuildingType.Main,        TimeSpan.FromSeconds(450) },
            { BuildingType.Market,      TimeSpan.FromSeconds(1350) },
            { BuildingType.Place,       TimeSpan.FromSeconds(5430) },
            { BuildingType.Smith,       TimeSpan.FromSeconds(3000) },
            { BuildingType.Snob,        TimeSpan.FromSeconds(293400) },
            { BuildingType.Stable,      TimeSpan.FromSeconds(3000) },
            { BuildingType.Statue,      TimeSpan.FromSeconds(750) },
            { BuildingType.Stone,       TimeSpan.FromSeconds(450) },
            { BuildingType.Storage,     TimeSpan.FromSeconds(510) },
            { BuildingType.Wall,        TimeSpan.FromSeconds(1800) },
            { BuildingType.Watchtower,  TimeSpan.FromSeconds(8800) },
            { BuildingType.Wood,        TimeSpan.FromSeconds(450) },
        };

        public static Dictionary<BuildingType, short> MaxLevels = new Dictionary<BuildingType, short>
        {
            { BuildingType.Barracks,    25 },
            { BuildingType.Church,      3 }, // May be incorrect depending on church world settings
            { BuildingType.Farm,        30 },
            { BuildingType.Garage,      15 },
            { BuildingType.Hide,        10 },
            { BuildingType.Iron,        30 },
            { BuildingType.Main,        30 },
            { BuildingType.Market,      25 },
            { BuildingType.Place,       1 },
            { BuildingType.Smith,       20 },
            { BuildingType.Snob,        1 }, // May be incorrect depending on academy world settings
            { BuildingType.Stable,      20 },
            { BuildingType.Statue,      1 },
            { BuildingType.Stone,       30 },
            { BuildingType.Storage,     30 },
            { BuildingType.Wall,        20 },
            { BuildingType.Watchtower,  20 },
            { BuildingType.Wood,        30 },
        };
    }
}
