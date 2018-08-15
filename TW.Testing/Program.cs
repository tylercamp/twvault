using System;
using System.Collections.Generic;
using TW.Vault.Features.Simulation;
using TW.Vault.Model.JSON;

namespace TW.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var attackingArmy = new Army
            {
                { "axe", 7000 },
                { "light", 3000 },
                { "ram", 220 },
                { "catapult", 75 }
            };

            var defendingArmy = new Army
            {
                { "spear", 10000 },
                { "sword", 10000 }
            };
            
            int testWallLevel = 15;

            var simulator = new BattleSimulator();
            var battleResults = simulator.SimulateAttack(attackingArmy, defendingArmy, testWallLevel);

            Console.WriteLine("Attacker results:");
            PrintArmy(attackingArmy, battleResults.AttackingArmy);

            Console.WriteLine("\nDefender results:");
            PrintArmy(defendingArmy, battleResults.DefendingArmy);

            Console.WriteLine("\nWall level: {0} - {1} = {2}", testWallLevel, testWallLevel - battleResults.NewWallLevel, battleResults.NewWallLevel);

            DateTime start = DateTime.UtcNow;
            int numNukes = 0;
            int wallLevel = testWallLevel;
            while (true)
            {
                var partialResults = simulator.SimulateAttack(attackingArmy, defendingArmy, wallLevel);
                wallLevel = partialResults.NewWallLevel;
                defendingArmy = partialResults.DefendingArmy;
                ++numNukes;

                if (defendingArmy.Count == 0)
                    break;
            }
            DateTime end = DateTime.UtcNow;

            Console.WriteLine("It would take {0} total attacks like this to defeat all defending troops.", numNukes);
            Console.WriteLine("Took {0}ms to simulate", (end - start).TotalMilliseconds);

            Console.ReadLine();
        }

        static void PrintArmy(Army army, Army newArmy)
        {
            foreach (var troop in army.Keys)
            {
                int originalCount = army[troop];
                int newCount = newArmy.GetValueOrDefault(troop, 0);

                Console.WriteLine("--- {0}: {1} - {2} = {3}", troop, originalCount, originalCount - newCount, newCount);
            }
        }
    }
}
