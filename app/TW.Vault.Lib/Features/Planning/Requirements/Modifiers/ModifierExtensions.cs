using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib.Features.Planning.Requirements.Modifiers
{
    public static class ModifierExtensions
    {
        public static ICommandRequirements LimitTroopType(this ICommandRequirements requirement, IEnumerable<Model.JSON.TroopType> troopTypes) =>
            new LimitTroopTypeModifier(requirement) { AllowedTypes = troopTypes.ToArray() };
    }
}
