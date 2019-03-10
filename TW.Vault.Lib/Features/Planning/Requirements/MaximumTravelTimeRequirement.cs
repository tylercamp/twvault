using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MaximumTravelTimeRequirement : ICommandRequirements
    {
        public TimeSpan MaximumTime { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            var calculator = new TravelCalculator(worldSpeed, travelSpeed);
            var fieldSpeed = calculator.ArmyFieldSpeed(army);
            return fieldSpeed * source.DistanceTo(target) <= MaximumTime;
        }
    }
}
