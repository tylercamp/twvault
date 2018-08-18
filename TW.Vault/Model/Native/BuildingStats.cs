using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Native
{
    public static class BuildingStats
    {
        public static Dictionary<BuildingType, short> MaxLevels = new Dictionary<BuildingType, short>
        {
            { BuildingType.Barracks, 25 },
            { BuildingType.Stables, 20 },
            { BuildingType.Garage, 15 }
        };

        //  TODO
        public static Dictionary<BuildingType, float[]> RecruitmentSpeedFactors = new Dictionary<BuildingType, float[]>
        {
            { BuildingType.Barracks, new [] { 1, 1.2f } },
            { BuildingType.Stables, new [] { 1, 1.2f } },
            { BuildingType.Garage, new [] { 1, 1.2f } }
        };
    }
}
