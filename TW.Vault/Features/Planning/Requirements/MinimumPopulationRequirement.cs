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
        public int MinimumPopulation { get; set; }

        public bool MeetsRequirement(Coordinate source, Coordinate target, Army army)
        {
            return ArmyStats.CalculateTotalPopulation(army) >= MinimumPopulation;
        }
    }
}
