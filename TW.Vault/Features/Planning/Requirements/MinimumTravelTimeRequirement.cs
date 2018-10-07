using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MinimumTravelTimeRequirement : ICommandRequirements
    {
        decimal worldSpeed, unitSpeed;

        public TimeSpan MinimumTime { get; set; }

        public MinimumTravelTimeRequirement(decimal worldSpeed, decimal unitSpeed)
        {
            this.worldSpeed = worldSpeed;
            this.unitSpeed = unitSpeed;
        }

        public bool MeetsRequirement(Coordinate source, Coordinate target, Army army)
        {
            var calculator = new TravelCalculator(worldSpeed, unitSpeed);
            var fieldSpeed = calculator.ArmyFieldSpeed(army);
            return fieldSpeed * source.DistanceTo(target) >= MinimumTime;
        }
    }
}
