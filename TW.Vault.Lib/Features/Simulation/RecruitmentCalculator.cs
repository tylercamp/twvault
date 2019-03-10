using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;
using JSON = TW.Vault.Model.JSON;
using Native = TW.Vault.Model.Native;

namespace TW.Vault.Features.Simulation
{
    public class RecruitmentCalculator
    {
        float worldSpeed;
        JSON.BuildingLevels buildingLevels;

        private static short DefaultBarracksLevel = 20;
        private static short DefaultStableLevel = 15;
        private static short DefaultWorkshopLevel = 5;
        private static short DefaultAcademyLevel = 1;

        private static List<JSON.TroopType> DefaultOffensiveBuild = new List<JSON.TroopType>
        {
            JSON.TroopType.Axe,
            JSON.TroopType.Light,
            JSON.TroopType.Ram,
            JSON.TroopType.Catapult
        };

        private static List<JSON.TroopType> DefaultDefensiveBuild = new List<JSON.TroopType>
        {
            JSON.TroopType.Spear, JSON.TroopType.Heavy
        };

        private static Dictionary<JSON.TroopType, int> DefaultOffensiveMaxTroops = new Dictionary<JSON.TroopType, int>
        {
            { JSON.TroopType.Light, 3200 },
            { JSON.TroopType.Ram, 220 },
            { JSON.TroopType.Catapult, 75 }
        };

        private static Dictionary<JSON.TroopType, int> DefaultDefensiveMaxTroops = new Dictionary<JSON.TroopType, int>
        {
            { JSON.TroopType.Spear, 8000 },
            { JSON.TroopType.Heavy, 2000 }
        };

        public int MaxPopulation { get; set; } = 21000;

        public RecruitmentCalculator(float worldSpeed, short barracksLevel, short stablesLevel, short workshopLevel, short academyLevel)
        {
            this.worldSpeed = worldSpeed;
            this.buildingLevels = new JSON.BuildingLevels();
            this.buildingLevels[Native.BuildingType.Barracks] = barracksLevel;
            this.buildingLevels[Native.BuildingType.Stable] = stablesLevel;
            this.buildingLevels[Native.BuildingType.Garage] = workshopLevel;
            this.buildingLevels[Native.BuildingType.Snob] = academyLevel;
        }

        public RecruitmentCalculator(float worldSpeed, JSON.BuildingLevels buildingLevels = null)
        {
            this.worldSpeed = worldSpeed;
            this.buildingLevels = buildingLevels ?? new JSON.BuildingLevels();

            if (!this.buildingLevels.ContainsKey(Native.BuildingType.Barracks))
                this.buildingLevels[Native.BuildingType.Barracks] = DefaultBarracksLevel;

            if (!this.buildingLevels.ContainsKey(Native.BuildingType.Stable))
                this.buildingLevels[Native.BuildingType.Stable] = DefaultStableLevel;

            if (!this.buildingLevels.ContainsKey(Native.BuildingType.Garage))
                this.buildingLevels[Native.BuildingType.Garage] = DefaultWorkshopLevel;

            if (!this.buildingLevels.ContainsKey(Native.BuildingType.Snob))
            {
                if (this.buildingLevels.ContainsKey(Native.BuildingType.Smith) && this.buildingLevels[Native.BuildingType.Smith] == Native.BuildingStats.MaxLevels[Native.BuildingType.Smith])
                    this.buildingLevels[Native.BuildingType.Snob] = DefaultAcademyLevel;
                else
                    this.buildingLevels[Native.BuildingType.Snob] = 0;
            }
        }

        public int CalculatePossibleUnitRecruitment(JSON.TroopType troopType, TimeSpan timeSpan)
        {
            var baseBuildTime = Native.ArmyStats.BaseRecruitTime[troopType];
            var unitSource = Native.ArmyStats.UnitSource[troopType];

            var buildingLevel = this.buildingLevels[unitSource];
            if (buildingLevel == 0)
                return 0;

            var recruitSpeedFactor = Native.BuildingStats.RecruitmentSpeedFactors[unitSource][buildingLevel - 1];
            var finalBuildTime = baseBuildTime * recruitSpeedFactor / this.worldSpeed;

            return (int)Math.Floor(timeSpan.TotalSeconds / finalBuildTime.TotalSeconds);
        }

        public JSON.Army CalculatePossibleArmyRecruitment(TimeSpan timeSpan, List<JSON.TroopType> troopTypes, Dictionary<JSON.TroopType, int> maxCounts = null)
        {
            //  Keep time span reasonable so we don't get integer overflow
            if (timeSpan.TotalDays > 30)
                timeSpan = TimeSpan.FromDays(30);

            var result = new JSON.Army();
            foreach (var type in troopTypes)
                result.Add(type, 0);

            var typesBySource = troopTypes.GroupBy(t => Native.ArmyStats.UnitSource[t]).ToDictionary((s) => s.Key, s => s.Select(t => t));

            //  Simulate recruitment by time interval
            var interval = TimeSpan.FromHours(6);
            var simulatedTime = TimeSpan.Zero;
            int totalPop = 0;
            int previousPop = -1;

            bool ReachedCountLimit(JSON.TroopType troopType)
            {
                return maxCounts != null && maxCounts.ContainsKey(troopType) && result[troopType] >= maxCounts[troopType];
            }

            while (simulatedTime < timeSpan && totalPop < MaxPopulation && totalPop != previousPop)
            {
                foreach (var source in typesBySource.Keys)
                {
                    var types = typesBySource[source].Where(t => !ReachedCountLimit(t));

                    var sumTimeSeconds = types.Sum(t => Native.ArmyStats.BaseRecruitTime[t].TotalSeconds);
                    foreach (var type in types)
                    {
                        var factor = 1 - Native.ArmyStats.BaseRecruitTime[type].TotalSeconds / sumTimeSeconds;

                        //  If there's only 1 unit type for this building
                        if (factor <= float.Epsilon)
                            factor = 1;

                        var troops = CalculatePossibleUnitRecruitment(type, interval * factor);
                        result[type] += troops;
                    }
                }

                if (maxCounts != null)
                {
                    foreach (var type in maxCounts.Keys)
                    {
                        if (!result.ContainsKey(type))
                            continue;

                        if (result[type] > maxCounts[type])
                            result[type] = maxCounts[type];
                    }
                }

                //  Keep track of previous/total in case we reach max counts before time limit or pop limit are reached
                //  (otherwise we may get infinite loop)
                previousPop = totalPop;
                totalPop = Native.ArmyStats.CalculateTotalPopulation(result);
                simulatedTime += interval;
            }

            return result;
        }

        public JSON.Army CalculatePossibleOffenseRecruitment(TimeSpan timeSpan) => CalculatePossibleArmyRecruitment(timeSpan, DefaultOffensiveBuild, DefaultOffensiveMaxTroops);
        public JSON.Army CalculatePossibleDefenseRecruitment(TimeSpan timeSpan) => CalculatePossibleArmyRecruitment(timeSpan, DefaultDefensiveBuild, DefaultDefensiveMaxTroops);
    }
}
