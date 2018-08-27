using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.JSON;
using TW.Vault.Model.Native;

namespace TW.Vault.Features.Simulation
{
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

        public TimeSpan CalculateConstructionTime(BuildingType buildingType, short hqLevel, short newLevel)
        {
            TimeSpan baseTime = BuildingStats.LevelOneBuildTimes[buildingType] * 1.18 * Math.Pow(BuildTimeFactor, newLevel - 1 - 14.0 / (newLevel - 1));
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
            while (timeSpan.Ticks > 0 && (numLevels + oldLevel + 1 <= BuildingStats.MaxLevels[buildingType]))
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
            foreach (var buildingType in BuildingStats.BuildingTypes.Where(t => BuildingStats.LevelOneBuildTimes[t].Ticks > 0))
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
