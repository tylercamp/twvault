using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TW.Vault.Features.Simulation;
using TW.Vault.Model.JSON;

namespace TW.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestBattleSimulation();
            //TestRecruitment();
            //TestUsingBattleTester();

            for (int i = 0; i < 10; i++)
                TestHighScores();

            Console.ReadLine();
        }

        static void TestHighScores()
        {
            Console.WriteLine("Making high-scores service");
            var highScoresService = new Vault.Features.HighScoresService(new Shim.ServiceScopeFactory(), new Shim.LoggerFactory());

            Dictionary<String, UserStats> generatedStats;

            Console.WriteLine("Starting");
            var start = DateTime.Now;
            highScoresService.WithVaultContext(async (ctx) =>
            {
                generatedStats = await highScoresService.GenerateHighScores(ctx, 1, 1, new CancellationToken());
            }).Wait();
            var time = DateTime.Now - start;

            Console.WriteLine("Done, took {0:N2} seconds", time.TotalSeconds);
        }



        static void TestUsingBattleTester()
        {
            var tester = new BattleTester("sim-test-data.json");
            var results = tester.Test();

            foreach (var result in results)
            {
                Console.WriteLine("{0}: {1} - {2:N2}% error {3}", result.Index, result.Passed ? "Passed" : "Failed", result.ErrorPercent * 100, result.Sample.Label ?? "");
                if (!result.Passed)
                {
                    var baseAttacker = result.Sample.Start.Attacker;
                    var expectedAttacker = result.Sample.Expect.Attacker;
                    var actualAttacker = result.ActualResult.Attacker;

                    var baseDefender = result.Sample.Start.Defender;
                    var expectedDefender = result.Sample.Expect.Defender;
                    var actualDefender = result.ActualResult.Defender;

                    var baseWall = result.Sample.Start.Wall;
                    var expectedWall = result.Sample.Expect.Wall;
                    var actualWall = result.ActualResult.Wall;

                    NormalizeArmies(baseAttacker, expectedAttacker, actualAttacker);
                    NormalizeArmies(baseDefender, expectedDefender, actualDefender);

                    Console.WriteLine("Attacker");
                    foreach (var troop in baseAttacker.Keys)
                        Console.WriteLine("--- {0} {1}: {2} (expected {3})", baseAttacker[troop], troop, actualAttacker[troop], expectedAttacker[troop]);

                    Console.WriteLine("Defender");
                    foreach (var troop in baseDefender.Keys)
                        Console.WriteLine("--- {0} {1}: {2} (expected {3})", baseDefender[troop], troop, actualDefender[troop], expectedDefender[troop]);

                    Console.WriteLine("Wall {0}: {1} (expected {2})", baseWall, actualWall, expectedWall);

                    Console.WriteLine();
                    Console.ReadLine();
                }
            }
        }

        static void TestRecruitment()
        {
            var calculator = new RecruitmentCalculator(2, 25, 20, 10, 1);

            for (int i = 0; i < 5; i++)
            {
                var timeSpan = TimeSpan.FromDays(3 * (i + 1));
                var defense = calculator.CalculatePossibleDefenseRecruitment(timeSpan);
                var offense = calculator.CalculatePossibleOffenseRecruitment(timeSpan);

                Console.WriteLine("\n\nIn {0} hours:", timeSpan.TotalHours);
                Console.WriteLine("[Offense]");
                PrintArmy(offense);

                Console.WriteLine("\n[Defense]");
                PrintArmy(defense);
            }
        }

        static void TestBattleSimulation()
        {
            var attackingArmy = new Army
            {
                { TroopType.Axe, 7000 },
                { TroopType.Light, 3000 },
                { TroopType.Ram, 220 },
                { TroopType.Catapult, 75 }
            };

            var defendingArmy = new Army
            {
                { TroopType.Spear, 10000 },
                { TroopType.Sword, 10000 }
            };

            int testWallLevel = 15;

            var simulator = new BattleSimulator();
            var battleResults = simulator.SimulateAttackWithoutArchers(attackingArmy, defendingArmy, testWallLevel);

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
                var partialResults = simulator.SimulateAttackWithoutArchers(attackingArmy, defendingArmy, wallLevel);
                wallLevel = partialResults.NewWallLevel;
                defendingArmy = partialResults.DefendingArmy;
                ++numNukes;

                if (defendingArmy.Count == 0)
                    break;
            }
            DateTime end = DateTime.UtcNow;

            Console.WriteLine("It would take {0} total attacks like this to defeat all defending troops.", numNukes);
            Console.WriteLine("Took {0}ms to simulate", (end - start).TotalMilliseconds);
        }

        static void PrintArmy(Army army)
        {
            foreach (var troop in army.Keys)
            {
                Console.WriteLine("--- {0}: {1}", troop, army[troop]);
            }
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

        static void NormalizeArmies(params Army[] armies)
        {
            var allKeys = armies.SelectMany(a => a.Keys.Where(k => a[k] > 0)).Distinct().ToList();
            foreach (var army in armies)
            {
                foreach (var key in allKeys.Where(k => !army.ContainsKey(k)))
                    army.Add(key, 0);
            }
        }
    }
}
