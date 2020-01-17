using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using TW.ConfigurationFetcher.Fetcher;
using TW.Vault.Scaffold;
using TW.Vault;
using ShellProgressBar;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TW.ConfigurationFetcher
{
    struct LocaleSettings
    {
        public short DefaultTranslationId { get; set; }
        public String TimeZoneId { get; set; }
    }

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

            var deleteOldWorlds = args.Contains("-clean") || true;
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

            String HostFor(String hostname) => String.Join('.', hostname.Split('.').Skip(1)).ToLower();
            String HostOf(World world) => HostFor(world.Hostname);

            using (vaultContext)
            using (httpClient)
            {
                var worlds = vaultContext.World.Include(w => w.WorldSettings).ToList();
                var hosts = worlds.Select(HostOf).Distinct().ToList();

                var hostLocales = hosts.ToDictionary(h => h, h =>
                {
                    var matchedWorlds = worlds.Where(w => HostOf(w) == h).ToList();
                    var distinctLocales = matchedWorlds
                        .Select(w => new LocaleSettings { DefaultTranslationId = w.DefaultTranslationId, TimeZoneId = w.WorldSettings.TimeZoneId })
                        .Distinct()
                        .ToList();

                    if (distinctLocales.Count > 1)
                        throw new Exception($"Expected one time zone for host {h}, got {distinctLocales.Count}");

                    return distinctLocales.First();
                });

                var activeWorldRegex = new Regex("\"https?\\:\\/\\/([^\"]+)\"");
                var hostActiveWorlds = new Dictionary<String, List<String>>();
                foreach (var host in hosts)
                {
                    Console.WriteLine("Fetching active servers for {0}...", host);
                    var serverListUrl = $"http://www.{host}/backend/get_servers.php";
                    var response = httpClient.GetStringAsync(serverListUrl).Result;
                    var activeWorldMatches = activeWorldRegex.Matches(response);
                    hostActiveWorlds.Add(host, activeWorldMatches.Select(m => m.Groups[1].Value).ToList());
                }

                var allWorldHostnames = hostActiveWorlds.SelectMany(kvp => kvp.Value).ToList();

                Console.WriteLine("Creating and pulling new worlds...");
                foreach (var hostname in allWorldHostnames)
                {
                    var world = worlds.Where(w => w.Hostname == hostname).FirstOrDefault();
                    if (world == null)
                    {
                        world = new World
                        {
                            Name = hostname.Split('.')[0],
                            Hostname = hostname,
                            // TODO - Auto-pull translation IDs based on language and assigned server type
                            DefaultTranslationId = hostLocales[HostFor(hostname)].DefaultTranslationId
                        };
                        vaultContext.Add(world);
                        vaultContext.SaveChanges();
                    }

                    var pendingFetchers = fetchers.Where(f => f.NeedsUpdate(world)).ToList();
                    if (pendingFetchers.Count > 0)
                        Console.WriteLine("Pulling for world {0}...", world.Hostname);

                    var baseUrl = $"https://{world.Hostname}";

                    foreach (var fetcher in pendingFetchers)
                    {
                        var url = $"{baseUrl}{fetcher.Endpoint}";
                        Console.Write("Fetching {0} ... ", url); Console.Out.Flush();

                        var response = httpClient.GetAsync(url).Result;
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                        {
                            Console.WriteLine("Warning: server {0} seems to have ended (redirection occurred)", world.Hostname);
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

                    var worldSettings = vaultContext.WorldSettings.Where(s => s.WorldId == world.Id).First();
                    worldSettings.TimeZoneId = hostLocales[HostFor(hostname)].TimeZoneId;
                    vaultContext.SaveChanges();
                }

                if (deleteOldWorlds)
                {
                    Console.WriteLine("Cleaning old world data...");
                    var oldWorlds = vaultContext.World.Where(w => !allWorldHostnames.Contains(w.Hostname)).Include(w => w.WorldSettings).ToList();
                    Console.WriteLine("Found {0} old worlds to be cleaned:", oldWorlds.Count);
                    foreach (var w in oldWorlds)
                        Console.WriteLine("- " + w.Hostname);

                    Console.ReadLine();

                    foreach (var w in oldWorlds)
                    {
                        List<Action<IProgressBar>> jobs = new List<Action<IProgressBar>>();
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.UserUploadHistory.Include(h => h.U).Where(h => h.U.WorldId == w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.User.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.UserLog.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Command.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CurrentBuilding.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CurrentVillage.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CurrentVillageSupport.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CurrentArmy.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CommandArmy.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.CurrentPlayer.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.EnemyTribe.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.IgnoredReport.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Report.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.ReportArmy.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.ReportBuilding.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Transaction.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Village.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Player.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Ally.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.Conquer.FromWorld(w.Id)));
                        jobs.Add((pb) => DeleteBatched(vaultContext, pb, vaultContext.AccessGroup.Where(g => g.WorldId == w.Id)));

                        var numJobsDone = 0;
                        String JobsProgressMessage() => $"Deleting data for {w.Hostname} (id={w.Id}) ({numJobsDone}/{jobs.Count} done)";

                        using (var dataProgressBar = new ProgressBar(jobs.Count, JobsProgressMessage()))
                        {
                            
                            foreach (var job in jobs)
                            {
                                using (var jobProgressBar = dataProgressBar.Spawn(1, ""))
                                    job(jobProgressBar);

                                numJobsDone++;
                                dataProgressBar.Tick(JobsProgressMessage());
                            }
                        }

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

            vaultContext.Dispose();

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }

        private static void DeleteBatched<T>(VaultContext context, IProgressBar progressBar, IQueryable<T> query) where T : class
        {
            var batchSize = 50;
            var didDelete = true;

            var numDeleted = 0;

            var numTotal = query.Count();

            String ProgressMessage() => $"Deleting {numTotal} entries of type {typeof(T).Name}... ({numDeleted}/{numTotal} done)";
            progressBar.Message = ProgressMessage();

            if (numTotal == 0)
            {
                progressBar.Tick(progressBar.MaxTicks);
                return;
            }

            progressBar.MaxTicks = numTotal;

            do
            {
                var batch = query.Take(batchSize).ToList();
                if (batch.Any())
                {
                    context.RemoveRange(batch);
                    context.SaveChanges();
                    numDeleted += batch.Count;

                    if (numDeleted > numTotal)
                    {
                        numTotal = query.Count();
                        progressBar.MaxTicks = numTotal;
                    }

                    progressBar.Tick(numDeleted, ProgressMessage());
                }
                else
                {
                    didDelete = false;
                }
            } while (didDelete);
        }
    }
}
