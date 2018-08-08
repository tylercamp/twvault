using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Simulation
{
    public enum BuildingType
    {
        Barracks,
        Church,
        Farm,
        Garage,
        Hide,
        Iron,
        Main,
        Market,
        Place,
        Smith,
        Snob,
        Stable,
        Statue,
        Stone,
        Storage,
        Wall,
        Watchtower,
        Wood
    }

    /* NOTE - LOTS OF HARD-CODED VALUES FOR EN100!! */
    public class ConstructionCalculator
    {
        private float worldSpeed;

        public ConstructionCalculator()
        {
            // en100 world speed
            this.worldSpeed = 2;
        }

        public ConstructionCalculator(float worldSpeed)
        {
            this.worldSpeed = worldSpeed;
        }

        private static float BuildTimeFactor = 1.2f;

        private static Dictionary<BuildingType, TimeSpan> LevelOneBuildTimes = new Dictionary<BuildingType, TimeSpan>
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
            { BuildingType.Watchtower,  TimeSpan.FromSeconds(0) }, // left at 0 since watchtowers disabled on en100
            { BuildingType.Wood,        TimeSpan.FromSeconds(450) },
        };

        public static Dictionary<BuildingType, int> MaxLevels = new Dictionary<BuildingType, int>
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

        public TimeSpan CalculateConstructionTime(BuildingType buildingType, short hqLevel, short newLevel)
        {
            TimeSpan baseTime = LevelOneBuildTimes[buildingType] * 1.18 * Math.Pow(BuildTimeFactor, newLevel - 1 - 14.0 / (newLevel - 1));
            TimeSpan timeWithHeadquarters = baseTime * Math.Pow(1.05, -hqLevel);

            return timeWithHeadquarters;
        }

        public TimeSpan CalculateConstructionTime(BuildingType buildingType, short hqLevel, short oldLevel, short newLevel)
        {
            TimeSpan totalTime = TimeSpan.Zero;
            for (int i = oldLevel + 1; i <= newLevel; i++)
                totalTime += CalculateConstructionTime(buildingType, hqLevel, (short)i);

            return totalTime;
        }

        public short CalculateLevelsInTimeSpan(BuildingType buildingType, short hqLevel, short oldLevel, TimeSpan timeSpan)
        {
            short numLevels = 0;
            while (timeSpan.Ticks > 0 && (numLevels + oldLevel + 1 <= MaxLevels[buildingType]))
            {
                TimeSpan nextLevelTime = CalculateConstructionTime(buildingType, hqLevel, (short)(oldLevel + numLevels + 1));
                timeSpan -= nextLevelTime;

                if (timeSpan.Ticks >= 0)
                    numLevels++;
            }
            return numLevels;
        }

        public TimeSpan CalculateConstructionTime(String buildingType, short hqLevel, short newLevel)
        {
            if (Char.IsLower(buildingType[0]))
                buildingType = Char.ToUpper(buildingType[0]) + buildingType.Substring(1);

            BuildingType enumType = Enum.Parse<BuildingType>(buildingType);
            return CalculateConstructionTime(enumType, hqLevel, newLevel);
        }

        public TimeSpan CalculateConstructionTime(String buildingType, short hqLevel, short oldLevel, short newLevel)
        {
            if (Char.IsLower(buildingType[0]))
                buildingType = Char.ToUpper(buildingType[0]) + buildingType.Substring(1);

            BuildingType enumType = Enum.Parse<BuildingType>(buildingType);
            return CalculateConstructionTime(enumType, hqLevel, oldLevel, newLevel);
        }

        public short CalculateLevelsInTimeSpan(String buildingType, short hqLevel, short oldLevel, TimeSpan timeSpan)
        {
            if (Char.IsLower(buildingType[0]))
                buildingType = Char.ToUpper(buildingType[0]) + buildingType.Substring(1);

            BuildingType enumType = Enum.Parse<BuildingType>(buildingType);
            return CalculateLevelsInTimeSpan(enumType, hqLevel, oldLevel, timeSpan);
        }

        public BuildingLevels CalculatePossibleBuildings(BuildingLevels oldLevels, TimeSpan timeSpan)
        {
            var hqLevel = oldLevels.GetValueOrDefault("main", (short)20);
            var result = new BuildingLevels();

            //  Get all valid building types for this world by name
            foreach (var buildingType in Enum.GetValues(typeof(BuildingType)).Cast<BuildingType>().Where(t => LevelOneBuildTimes[t].Ticks > 0))
            {
                String buildingName = buildingType.ToString().ToLower();

                short currentLevel = oldLevels.GetValueOrDefault(buildingName, (short)0);
                short possibleLevels = CalculateLevelsInTimeSpan(buildingType, hqLevel, currentLevel, timeSpan);
                result[buildingName] = (short)(currentLevel + possibleLevels);
            }

            return result;
        }
    }
}
