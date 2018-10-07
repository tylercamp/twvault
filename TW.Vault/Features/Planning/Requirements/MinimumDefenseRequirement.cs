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
        public int MinimumDefense { get; set; }

        public bool MeetsRequirement(Coordinate source, Coordinate target, Army army)
        {
            return BattleSimulator.DefensePower(army).Total >= MinimumDefense;
        }
    }
}
