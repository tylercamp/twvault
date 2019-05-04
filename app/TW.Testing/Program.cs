using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TW.Vault.Features.Simulation;
using TW.Vault.Model.JSON;
using TW.Vault;
using Microsoft.EntityFrameworkCore;
using TW.Vault.Model.Native;

namespace TW.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestBattleSimulation();
            //TestRecruitment();
            //TestUsingBattleTester();

            //for (int i = 0; i < 10; i++)
            //    TestHighScores();

            //DoSomeQuery();
            TestTravelTime();

            Console.ReadLine();
        }

        static void TestTravelTime()
        {
            var calculator = new TravelCalculator(1.0f, 1.0f);
            var start = new Village { X = 200, Y = 200 };
            var end = new Village { X = 210, Y = 220 };

            var troopsWithTimes = Enum.GetValues(typeof(TroopType)).Cast<TroopType>().Where(t => t != TroopType.Militia).ToDictionary(t => t, t => calculator.CalculateTravelTime(t, start, end));

            var offsets = new[] { TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(60) };

            foreach (var o in offsets)
            {
                Console.WriteLine("With offset of {0} minutes", o.TotalMinutes);
                foreach (var kvp in troopsWithTimes)
                {
                    var type = kvp.Key;
                    var time = kvp.Value;
                    var modifiedTime = time - o;

                    var estimate = calculator.EstimateTroopType(modifiedTime, start, end);
                    Console.WriteLine("- For {0} (speed={1}) estimated as {2} (speed={3})", type, ArmyStats.TravelSpeed[type], estimate, ArmyStats.TravelSpeed[estimate]);
                }
            }
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


        static void DoSomeQuery()
        {
            var coords = File.ReadAllLines("coords.txt")
                .Select(l => l.Trim())
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .Select(l => l.Split('|'))
                .Select(c => new { X = short.Parse(c[0]), Y = short.Parse(c[1]) })
                .ToList();

            using (var context = new Shim.ServiceScopeFactory().CreateScope().ServiceProvider.GetRequiredService<Vault.Scaffold.VaultContext>())
            {
                var villages = (
                        from village in context.Village.FromWorld(1)
                        where coords.Contains(new { X = village.X.Value, Y = village.Y.Value })
                        select village
                    ).ToList();

                var villageIds = villages.Select(v => v.VillageId).ToList();

                var currentVillages = context.CurrentVillage
                    .Include(cv => cv.ArmyStationed)
                    .Include(cv => cv.ArmyOwned)
                    .Include(cv => cv.ArmyTraveling)
                    .FromWorld(1)
                    .FromAccessGroup(1)
                    .Where(cv => villageIds.Contains(cv.VillageId))
                    .ToList();

                var commands = context.Command.FromWorld(1).FromAccessGroup(1).Where(cmd => villageIds.Contains(cmd.SourceVillageId)).Where(cmd => cmd.LandsAt > DateTime.UtcNow).ToList();
                //var attackedPlayers = 

                var oldestTimesByVillage = currentVillages.ToDictionary(
                    cv => cv.VillageId,
                    cv => new [] { cv.ArmyStationed?.LastUpdated, cv.ArmyOwned?.LastUpdated, cv.ArmyTraveling?.LastUpdated }.Where(d => d != null).OrderByDescending(d => d.Value).FirstOrDefault() ?? DateTime.MinValue
                );

                var oldVillages = oldestTimesByVillage.Where(kvp => DateTime.UtcNow - kvp.Value > TimeSpan.FromDays(7));

                var villagesById = villages.ToDictionary(v => v.VillageId, v => v);

                var rows = oldVillages.Select(kvp => new
                {
                    X = villagesById[kvp.Key].X.Value,
                    Y = villagesById[kvp.Key].Y.Value
                }).ToList();

                String result = "sep=,\nX,Y\n";
                result += String.Join('\n', rows.Select(r => r.X + "," + r.Y));

                File.WriteAllText("result.csv", result);
            }
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
