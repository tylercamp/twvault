using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TW.Vault.Lib.Features.Simulation;
using TW.Vault.Lib.Model.JSON;
using Microsoft.EntityFrameworkCore;
using TW.Vault.Lib.Model.Native;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using TW.Vault.Lib;

namespace TW.Testing
{
    static class Program
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
            //CleanDuplicateReports();

            Console.ReadLine();
        }

        struct ReportSignature
        {
            public ReportSignature(Vault.Lib.Scaffold.Report report)
            {
                AttackerPlayerId = report.AttackerPlayerId;
                DefenderPlayerId = report.DefenderPlayerId;
                AttackerVillageId = report.AttackerVillageId;
                DefenderVillageId = report.DefenderVillageId;
                OccurredAt = report.OccuredAt;
                AttackerArmy = report.AttackerArmy;
                AttackerArmyLosses = report.AttackerLossesArmy;
                DefenderArmy = report.DefenderArmy;
                DefenderArmyLosses = report.DefenderLossesArmy;
                DefenderTravelingArmy = report.DefenderTravelingArmy;
                AccessGroupId = report.AccessGroupId;
                WorldId = report.WorldId;
            }

            public long? AttackerPlayerId, DefenderPlayerId, AttackerVillageId, DefenderVillageId;
            public DateTime OccurredAt;
            public Army AttackerArmy, AttackerArmyLosses, DefenderArmy, DefenderArmyLosses, DefenderTravelingArmy;
            public int AccessGroupId, WorldId;
        }

        public static List<List<V>> FastGroupByUnordered<K,V>(this IEnumerable<V> collection, Func<V,K> selector)
        {
            var result = new Dictionary<K, List<V>>();
            foreach (var item in collection)
            {
                var id = selector(item);
                if (!result.ContainsKey(id))
                    result[id] = new List<V>();
                result[id].Add(item);
            }
            return result.Values.ToList();
        }

        static List<List<V>> ThenFastGroupByUnordered<K,V>(this IEnumerable<List<V>> collections, Func<V,K> selector, int minSize = 32)
        {
            return collections.SelectMany(c => c.Count < minSize ? new List<List<V>> { c } : c.FastGroupByUnordered(selector)).ToList();
        }

        static void CleanDuplicateReports()
        {
            Console.WriteLine("Creating connection...");

            using (var context = new Vault.Lib.Scaffold.VaultContext(
                    new DbContextOptionsBuilder<Vault.Lib.Scaffold.VaultContext>()
                        .UseNpgsql("Server=v.tylercamp.me; Port=22342; Database=vault; User Id=twu_vault; Password=!!TWV@ult4Us??")
                        .Options
                ))
            {
                var sw = new Stopwatch();

                Console.WriteLine("Getting player IDs...");
                var playerIds = context.User.OrderBy(g => g.PlayerId).Select(g => g.PlayerId).ToList().Distinct().ToList();
                var playerReports = new ConcurrentBag<List<Vault.Lib.Scaffold.Report>>();

                sw.Restart();
                Console.WriteLine("Getting all reports...");
                int numPlayersLoaded = 0;
                Parallel.ForEach(playerIds, new ParallelOptions { MaxDegreeOfParallelism = 64 }, (id) =>
                {
                    using (var playerContext = new Vault.Lib.Scaffold.VaultContext(
                        new DbContextOptionsBuilder<Vault.Lib.Scaffold.VaultContext>()
                            .UseNpgsql("Server=v.tylercamp.me; Port=22342; Database=vault; User Id=twu_vault; Password=!!TWV@ult4Us??")
                            .Options
                    ))
                    {
                        playerReports.Add(playerContext.Report.IncludeReportData().AsNoTracking().Where(r => r.AttackerPlayerId == id).ToList());
                    }

                    Interlocked.Increment(ref numPlayersLoaded);
                    lock (context)
                        Console.Title = "Loaded " + numPlayersLoaded + "/" + playerIds.Count;
                });

                Console.WriteLine("Got {0} reports in {1}s", playerReports.DefaultIfEmpty(new List<Vault.Lib.Scaffold.Report>()).Sum(l => l.Count), sw.ElapsedMilliseconds / 1000);

                sw.Restart();
                Console.Write("Pre-categorizing reports... ");
                var categorizedReports = playerReports
                    .ThenFastGroupByUnordered(r => r.AccessGroupId)
                    .ThenFastGroupByUnordered(r => r.AttackerVillageId)
                    .ThenFastGroupByUnordered(r => r.DefenderVillageId)
                    .ThenFastGroupByUnordered(r => r.OccuredAt);
                playerReports = null;
                GC.Collect();
                Console.WriteLine("Took {0}ms", sw.ElapsedMilliseconds);

                sw.Restart();
                Console.Write("Generating concurrent set... ");
                var reportsQueue = new BlockingCollection<List<Vault.Lib.Scaffold.Report>>(new ConcurrentBag<List<Vault.Lib.Scaffold.Report>>(categorizedReports));
                reportsQueue.CompleteAdding();
                categorizedReports = null;
                GC.Collect();
                Console.WriteLine("Took {0}ms", sw.ElapsedMilliseconds);

                int totalJobs = reportsQueue.Count;
                var allDuplicates = new ConcurrentBag<List<Vault.Lib.Scaffold.Report>>();

                sw.Restart();
                Console.Write("Checking for duplicates within " + reportsQueue.Count + " categories... ");
                for (int i = 0; i < 32; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        foreach (var group in reportsQueue.GetConsumingEnumerable())
                        {
                            var reportsBySignature = new Dictionary<ReportSignature, List<Vault.Lib.Scaffold.Report>>();
                            foreach (var report in group)
                            {
                                var sign = new ReportSignature(report);
                                if (!reportsBySignature.ContainsKey(sign))
                                    reportsBySignature[sign] = new List<Vault.Lib.Scaffold.Report>();
                                reportsBySignature[sign].Add(report);
                            }

                            foreach (var set in reportsBySignature.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Value))
                                allDuplicates.Add(set);
                        }
                    }, TaskCreationOptions.LongRunning);
                }

                while (!reportsQueue.IsCompleted)
                {
                    Thread.Sleep(1000);
                    Console.Title = reportsQueue.Count + "/" + totalJobs + " remaining";
                }

                Console.WriteLine("Took {0}m {1}s", (int)sw.Elapsed.TotalMinutes, sw.Elapsed.Seconds);
                Console.WriteLine("Total of {0} reports that were duplicated ({1} total duplicate reports)", allDuplicates.Count, allDuplicates.DefaultIfEmpty(new List<Vault.Lib.Scaffold.Report>()).Sum(l => l.Count));

                sw.Restart();
                var updateSw = Stopwatch.StartNew();
                Console.Write("Verifying duplicates... ");
                int numChecked = 0;
                foreach (var group in allDuplicates)
                {
                    var reference = group[0];
                    if (!group.All(r =>
                        (Army)r.DefenderArmy == (Army)reference.DefenderArmy &&
                        (Army)r.AttackerArmy == (Army)reference.AttackerArmy &&
                        (Army)r.DefenderLossesArmy == (Army)reference.DefenderLossesArmy &&
                        (Army)r.AttackerLossesArmy == (Army)reference.AttackerLossesArmy &&
                        (Army)r.DefenderTravelingArmy == (Army)reference.DefenderTravelingArmy &&
                        r.AttackerPlayerId == reference.AttackerPlayerId &&
                        r.DefenderPlayerId == reference.DefenderPlayerId &&
                        r.AttackerVillageId == reference.AttackerVillageId &&
                        r.DefenderVillageId == reference.DefenderVillageId &&
                        // Loyalty null in some cases
                        //r.Loyalty == reference.Loyalty &&
                        r.OccuredAt == reference.OccuredAt &&
                        // Luck null or mismatched in some cases
                        //r.Luck == reference.Luck &&
                        r.WorldId == reference.WorldId &&
                        r.AccessGroupId == reference.AccessGroupId
                    ))
                    {
                        Console.WriteLine("Invalid duplicate set found");
                        Debugger.Break();
                    }

                    ++numChecked;

                    if (updateSw.ElapsedMilliseconds >= 1000)
                    {
                        Console.Title = "Checked " + numChecked + "/" + allDuplicates.Count;
                        updateSw.Restart();
                    }
                }

                Console.WriteLine("Took {0}m {1}s", (int)sw.Elapsed.TotalMinutes, sw.Elapsed.Seconds);

                Console.Write("Determining best copies and removable reports... ");
                sw.Restart();
                var removableReports = new List<Vault.Lib.Scaffold.Report>();
                foreach (var group in allDuplicates)
                {
                    var scores = group.Select(r => { return new { Score = r.Loyalty != null ? 1 : 0, Report = r }; }).OrderByDescending(s => s.Score);
                    var best = scores.First();
                    var others = group.Where(r => r != best.Report);
                    removableReports.AddRange(others);
                }
                Console.WriteLine("Took {0}ms", sw.ElapsedMilliseconds);

                Console.WriteLine("Press Enter to delete the {0} removed entries.", removableReports.Count);
                Console.ReadLine();

                Console.WriteLine("Deleting... ");
                int numCleared = 0;
                sw.Restart();
                foreach (var group in removableReports.Grouped(1500).Select(g => g.ToList()))
                {
                    context.RemoveRange(group);
                    context.SaveChanges();
                    numCleared += group.Count;
                    Console.Title = "Cleared " + numCleared + "/" + removableReports.Count;
                }
                Console.WriteLine("Took {0}ms", sw.ElapsedMilliseconds);
            }
        }

        static void TestTravelTime()
        {
            var calculator = new TravelCalculator(1.0f, 1.0f);
            var start = new Village { X = 200, Y = 200 };
            var end = new Village { X = 210, Y = 220 };

            var start2 = new Village { X = 541, Y = 533 };
            var end2 = new Village { X = 494, Y = 533 };

            var calc2 = new TravelCalculator(1.3f, 0.7f);
            var extraTime = calc2.EstimateTroopType(
                TimeSpan.FromSeconds(74804),
                start2,
                end2
            );

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
            var highScoresService = new Vault.Lib.Features.HighScoresService(new Shim.ServiceScopeFactory(), new Shim.LoggerFactory());

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

            using (var context = new Shim.ServiceScopeFactory().CreateScope().ServiceProvider.GetRequiredService<Vault.Lib.Scaffold.VaultContext>())
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
