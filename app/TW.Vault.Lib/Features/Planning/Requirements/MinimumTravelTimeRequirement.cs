using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Features.Simulation;
using TW.Vault.Lib.Model;
using TW.Vault.Lib.Model.JSON;

namespace TW.Vault.Lib.Features.Planning.Requirements
{
    public class MinimumTravelTimeRequirement : ICommandRequirements
    {
        public TimeSpan MinimumTime { get; set; }

        public bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Army army)
        {
            var calculator = new TravelCalculator(worldSpeed, travelSpeed);
            var fieldSpeed = calculator.ArmyFieldSpeed(army);
            return fieldSpeed * source.DistanceTo(target) >= MinimumTime;
        }
    }
}
