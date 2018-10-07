using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MaximumDistanceRequirement : ICommandRequirements
    {
        public float MaximumDistance { get; set; }

        public bool MeetsRequirement(Coordinate source, Coordinate target, Army army)
        {
            return source.DistanceTo(target) <= MaximumDistance;
        }
    }
}
