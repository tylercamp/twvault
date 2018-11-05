using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MinimumDefenseRequirement : ICommandRequirements
    {
        public static readonly MinimumDefenseRequirement FullDV = new MinimumDefenseRequirement { MinimumDefense = 1700000 };
        public static readonly MinimumDefenseRequirement ThreeQuarterDV = new MinimumDefenseRequirement { MinimumDefense = 1275000 };
        public static MinimumPopulationRequirement HalfDV = new MinimumPopulationRequirement { MinimumPopulation = 850000 };
        public static MinimumPopulationRequirement QuarterDV = new MinimumPopulationRequirement { MinimumPopulation = 425000 };

        public int MinimumDefense { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            return BattleSimulator.DefensePower(army).Total >= MinimumDefense;
        }
    }
}
