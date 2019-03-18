using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using TW.ConfigurationFetcher.Fetcher;
using TW.Vault.Scaffold;
using TW.Vault;

namespace TW.ConfigurationFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Connection string must be provided as first argument");
                Environment.ExitCode = 1;
                return;
            }

            var connectionString = args[0];
            var worlds = args.Skip(1).Select(a => a.ToLower()).Where(a => !a.StartsWith("-")).ToArray();

#if DEBUG
            if (worlds.Length == 0)
                worlds = new[] { "en100.tribalwars.net" };
#endif

            if (worlds.Length == 0)
            {
                Console.WriteLine("No server worlds specified");
                Environment.ExitCode = 1;
                return;
            }

            var invalidHostnames = worlds.Where(h => !Regex.IsMatch(h, @"^\w+\.\w+\.\w+$")).ToList();
            if (invalidHostnames.Any())
            {
                Console.WriteLine("Invalid worlds were given, they should be e.g. en100.tribalwars.net");
                foreach (var hostname in invalidHostnames)
                    Console.WriteLine("- {0}", hostname);
                Environment.ExitCode = 1;
                return;
            }

            var deleteOldWorlds = args.Contains("-clean");
            Console.WriteLine("deleteOldWorlds ('-clean'): " + deleteOldWorlds);

            var vaultContext = new VaultContext(
                new DbContextOptionsBuilder<VaultContext>()
                    .UseNpgsql(connectionString)
                    .Options
            );

            var fetchers = new IFetcher[]
            {
                new BuildingFetcher(),
                new ConfigFetcher(),
                new UnitFetcher()
            };

            var httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false
            });

            using (vaultContext)
            using (httpClient)
            {
                Console.WriteLine("Creating and pulling new worlds...");
                foreach (var hostname in worlds)
                {
                    var world = vaultContext.World
                        .Include(w => w.WorldSettings)
                        .Where(w => w.Hostname == hostname)
                        .FirstOrDefault();

                    if (world == null)
                    {
                        world = new World
                        {
                            Name = hostname.Split('.')[0],
                            Hostname = hostname,
                            // TODO - Auto-pull translation IDs based on language and assigned server type
                            DefaultTranslationId = 1
                        };
                        vaultContext.Add(world);
                        vaultContext.SaveChanges();
                    }

                    Console.WriteLine("Pulling for world {0}...", hostname);

                    var baseUrl = $"https://{hostname}";

                    foreach (var fetcher in fetchers.Where(f => f.NeedsUpdate(world)))
                    {
                        var url = $"{baseUrl}{fetcher.Endpoint}";
                        Console.Write("Fetching {0} ... ", url); Console.Out.Flush();

                        var response = httpClient.GetAsync(url).Result;
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                        {
                            Console.WriteLine("Warning: server {0} seems to have ended (redirection occurred)", hostname);
                            break;
                        }
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("ERROR: Request failed with code {0}", response.StatusCode);
                            Environment.ExitCode = 1;
                            continue;
                        }

                        Console.Write("Processing... "); Console.Out.Flush();

                        var data = response.Content.ReadAsStringAsync().Result;
                        fetcher.Process(vaultContext, world, data);

                        Console.WriteLine("Done.");
                    }
                }

                if (deleteOldWorlds)
                {
                    Console.WriteLine("Cleaning old world data...");
                    var oldWorlds = vaultContext.World.Where(w => !worlds.Contains(w.Hostname)).Include(w => w.WorldSettings).ToList();
                    Console.WriteLine("Found {0} old worlds to be cleaned:", oldWorlds.Count);
                    foreach (var w in oldWorlds)
                        Console.WriteLine("- " + w.Hostname);

                    foreach (var w in oldWorlds)
                    {
                        Console.WriteLine("Deleting data for {0}...", w.Hostname);
                        
                        DeleteBatched(vaultContext, vaultContext.UserUploadHistory.Include(h => h.U).Where(h => h.U.WorldId == w.Id));
                        DeleteBatched(vaultContext, vaultContext.User.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.UserLog.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Command.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CurrentBuilding.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CurrentVillage.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CurrentVillageSupport.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CurrentArmy.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CommandArmy.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.CurrentPlayer.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.EnemyTribe.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.IgnoredReport.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Report.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.ReportArmy.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.ReportBuilding.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Transaction.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Village.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Player.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Ally.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.Conquer.FromWorld(w.Id));
                        DeleteBatched(vaultContext, vaultContext.AccessGroup.Where(g => g.WorldId == w.Id));

                        Console.WriteLine("Deleting world settings...");
                        vaultContext.Remove(w.WorldSettings);
                        vaultContext.SaveChanges();

                        Console.WriteLine("Deleting world...");
                        vaultContext.Remove(w);
                        vaultContext.SaveChanges();

                        Console.WriteLine("Deleted all data for {0}.", w.Hostname);
                    }
                }
            }

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }

        private static void DeleteBatched<T>(VaultContext context, IQueryable<T> query)
        {
            var batchSize = 1000;
            var didDelete = true;

            var numDeleted = 0;

            var numTotal = query.Count();
            Console.Write("Deleting {0} entries of type {1}... ", numTotal, typeof(T).Name); Console.Out.Flush();

            var lastOutputCount = 0;

            do
            {
                var batch = query.Take(batchSize).ToList();
                if (batch.Any())
                {
                    context.Remove(batch);
                    context.SaveChangesAsync();
                    numDeleted += batch.Count;

                    // Write progress every 20% or so
                    if ((numDeleted - lastOutputCount) / (float)numTotal > 0.2f)
                    {
                        lastOutputCount = numDeleted;
                        Console.Write("{0}%... ", numDeleted * 100 / numTotal); Console.Out.Flush();
                    }
                }
                else
                {
                    didDelete = false;
                }
            } while (didDelete);

            Console.WriteLine("Done.");
        }
    }
}
