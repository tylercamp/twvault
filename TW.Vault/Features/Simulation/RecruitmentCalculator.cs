using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;
using Native = TW.Vault.Model.Native;

namespace TW.Vault.Features.Simulation
{
    public class RecruitmentCalculator
    {
        float worldSpeed;
        int barracksLevel, stablesLevel, workshopLevel;

        public RecruitmentCalculator(float worldSpeed)
        {
            this.worldSpeed = worldSpeed;
        }

        public RecruitmentCalculator(float worldSpeed, int barracksLevel, int stablesLevel, int workshopLevel)
        {
            this.worldSpeed = worldSpeed;
            this.barracksLevel = barracksLevel;
            this.stablesLevel = stablesLevel;
            this.workshopLevel = workshopLevel;
        }

        public RecruitmentCalculator(float worldSpeed, JSON.BuildingLevels buildingLevels)
        {
            this.worldSpeed = worldSpeed;
            this.barracksLevel = buildingLevels.GetValueOrMax(Native.BuildingType.Barracks);
            this.stablesLevel = buildingLevels.GetValueOrMax(Native.BuildingType.Stables);
            this.workshopLevel = buildingLevels.GetValueOrMax(Native.BuildingType.Garage);
        }
    }
}
