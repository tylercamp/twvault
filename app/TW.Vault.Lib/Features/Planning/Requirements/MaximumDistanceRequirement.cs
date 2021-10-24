using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Model;
using TW.Vault.Lib.Model.JSON;

namespace TW.Vault.Lib.Features.Planning.Requirements
{
    public class MaximumDistanceRequirement : ICommandRequirements
    {
        public float MaximumDistance { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            return source.DistanceTo(target) <= MaximumDistance;
        }
    }
}
