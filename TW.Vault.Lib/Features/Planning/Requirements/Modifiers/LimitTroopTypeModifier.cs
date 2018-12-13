using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements.Modifiers
{
    public class LimitTroopTypeModifier : ICommandRequirements
    {
        ICommandRequirements innerRequirement;
        public LimitTroopTypeModifier(ICommandRequirements innerRequirement)
        {
            this.innerRequirement = innerRequirement;
        }

        public TroopType[] AllowedTypes { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            return innerRequirement.MeetsRequirement(worldSpeed, travelSpeed, source, target, army.Only(AllowedTypes));
        }
    }
}
