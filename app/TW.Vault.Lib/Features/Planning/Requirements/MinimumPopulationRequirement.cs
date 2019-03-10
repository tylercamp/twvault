using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;
using TW.Vault.Model.JSON;
using TW.Vault.Model.Native;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MinimumPopulationRequirement : ICommandRequirements
    {
        public static MinimumPopulationRequirement FullVillage = new MinimumPopulationRequirement { MinimumPopulation = 20000 };
        public static MinimumPopulationRequirement ThreeQuarterVillage = new MinimumPopulationRequirement { MinimumPopulation = 15000 };
        public static MinimumPopulationRequirement HalfVillage = new MinimumPopulationRequirement { MinimumPopulation = 10000 };
        public static MinimumPopulationRequirement QuarterVillage = new MinimumPopulationRequirement { MinimumPopulation = 5000 };


        public int MinimumPopulation { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            return ArmyStats.CalculateTotalPopulation(army) >= MinimumPopulation;
        }
    }
}
