using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features.Planning.Requirements;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.Convert;
using TW.Vault.Model.JSON;
using TW.Vault.Model.Native;

namespace TW.Vault.Features.Planning
{
    public class CommandOptionsCalculator
    {
        decimal worldSpeed, unitSpeed;

        public List<ICommandRequirements> Requirements { get; } = new List<ICommandRequirements>();

        public CommandOptionsCalculator(Scaffold.WorldSettings settings)
        {
            this.worldSpeed = settings.GameSpeed;
            this.unitSpeed = settings.UnitSpeed;
        }

        public CommandOptionsCalculator(decimal worldSpeed, decimal unitSpeed)
        {
            this.worldSpeed = worldSpeed;
            this.unitSpeed = unitSpeed;
        }

        public List<CommandInstruction> GenerateOptions(Dictionary<Scaffold.Village, Scaffold.CurrentVillage> availableVillages, Scaffold.Village target)
        {
            var result = new List<CommandInstruction>();
            var travelCalculator = new TravelCalculator(worldSpeed, unitSpeed);

            if (availableVillages == null)
                throw new ArgumentNullException(nameof(availableVillages));

            foreach ((var source, var currentVillage) in availableVillages.Tupled())
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (currentVillage == null)
                    throw new ArgumentNullException("currentVillage");

                var villageArmy = ArmyConvert.ArmyToJson(currentVillage.ArmyAtHome);
                if (villageArmy == null)
                    throw new ArgumentNullException("villageArmy");
                if (villageArmy.IsEmpty())
                    continue;

                foreach (var permutation in ArmyPermutations(villageArmy))
                {
                    if (Requirements.Any(r => !r.MeetsRequirement(worldSpeed, unitSpeed, source.Coordinates(), target.Coordinates(), permutation)))
                        continue;

                    var travelTroopType = travelCalculator.TravelTroopType(permutation);

                    result.Add(new CommandInstruction
                    {
                        SendFrom = source.VillageId,
                        SendTo = target.VillageId,
                        TroopType = travelTroopType,
                        TravelTime = travelCalculator.CalculateTravelTime(travelTroopType, source, target)
                    });
                }
            }

            return result;
        }


        IEnumerable<Army> ArmyPermutations(Army army)
        {
            var possibleSpeeds = army.Where(kvp => kvp.Value > 0).Select(kvp => ArmyStats.TravelSpeed[kvp.Key]).Distinct();
            foreach (var speed in possibleSpeeds)
            {
                var permutation = new Army();
                foreach (var unit in army.Where(kvp => ArmyStats.TravelSpeed[kvp.Key] <= speed))
                    permutation.Add(unit.Key, unit.Value);

                yield return permutation;
            }
        }

    }
}
