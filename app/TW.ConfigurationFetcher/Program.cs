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
using Npgsql;
using System.Diagnostics;
using System.Threading;

namespace TW.ConfigurationFetcher
{
    class LocaleSettings
    {
        public short DefaultTranslationId { get; set; }
        public String TimeZoneId { get; set; }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return false;

            var asSettings = obj as LocaleSettings;
            if (asSettings == null)
                return false;

            return TimeZoneId == asSettings.TimeZoneId && DefaultTranslationId == asSettings.DefaultTranslationId;
        }

        public override int GetHashCode()
        {
            return DefaultTranslationId.GetHashCode() ^ TimeZoneId.GetHashCode();
        }
    }

    class Program
    {
        private static String HostFor(String hostname) => String.Join('.', hostname.Split('.').Skip(1)).ToLower();
        private static String HostOf(World world) => HostFor(world.Hostname);

        private static HttpClient _httpClient = null;
        private static HttpClient httpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = HttpClientFactory.Create(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false, Proxy = null });
                return _httpClient;
            }
        }

        static void Main(string[] args)
        {
            var config = Config.ParseParams(args);
            if (!config.IsValid)
            {
                Console.WriteLine(Config.UsageDescription);
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine("Using config:\n" + config.ToString());

            var connectionString = config.ConnectionString;
            var extraTLDs = config.ExtraTLDs;
            var extraServers = config.ExtraServers;
            var deleteOldWorlds = config.Clean;
            var fetchNewServers = config.FetchTldServers;

            var vaultContext = new VaultContext(
                new DbContextOptionsBuilder<VaultContext>()
                    .UseNpgsql(connectionString, opt => opt.CommandTimeout((int)TimeSpan.FromMinutes(180).TotalSeconds))
                    .Options
            );

            using (vaultContext)
            {
                var worlds = vaultContext.World.Where(w => !w.IsPendingDeletion).Include(w => w.WorldSettings).ToList();
                var tlds = worlds.Select(HostOf).Concat(extraTLDs).Distinct().ToList();

                var hostLocales = tlds.ToDictionary(h => h, h =>
                {
                    var matchedWorlds = worlds.Where(w => HostOf(w) == h).ToList();
                    var distinctLocales = matchedWorlds
                        .Select(w => new LocaleSettings { DefaultTranslationId = w.DefaultTranslationId, TimeZoneId = w.WorldSettings.TimeZoneId })
                        .Distinct()
                        .ToList();

                    if (distinctLocales.Count > 1)
                        throw new Exception($"Expected one time zone for host {h}, got {distinctLocales.Count}");

                    return distinctLocales.FirstOrDefault();
                });

                foreach (var tld in tlds)
                {
                    if (hostLocales[tld] == null)
                        hostLocales.Remove(tld);
                }

                foreach (var template in Vault.Scaffold.Seed.WorldSettingsTemplate.Templates)
                {
                    if (!hostLocales.ContainsKey(template.TldHostname) || hostLocales[template.TldHostname] == null)
                        hostLocales[template.TldHostname] = new LocaleSettings { TimeZoneId = template.TimeZoneId, DefaultTranslationId = template.DefaultTranslationId };
                }

                var activeWorldHostnames = tlds.SelectMany(h => FetchAvailableWorlds(h)).ToList();

                var allWorldHostnames =
                    extraServers.Concat(fetchNewServers
                        ? activeWorldHostnames
                        : worlds.Select(w => w.Hostname)
                    ).Distinct().ToList();

                Console.WriteLine($"Creating and pulling {allWorldHostnames.Count} worlds...");
                foreach (var hostname in allWorldHostnames)
                    FetchWorldData(vaultContext, worlds, hostLocales, hostname, config.FetchExisting);

                if (deleteOldWorlds)
                {
                    Console.WriteLine("Cleaning old world data...");
                    var oldWorlds = vaultContext.World.Include(w => w.WorldSettings).ToList().Where(w => w.IsPendingDeletion || !activeWorldHostnames.Contains(w.Hostname)).ToList();
                    Console.WriteLine("Found {0} old worlds to be cleaned:", oldWorlds.Count);
                    foreach (var w in oldWorlds)
                        Console.WriteLine("- " + w.Hostname);

                    Console.WriteLine("Press Enter to continue.");
                    Console.ReadLine();

                    Console.WriteLine("Marking worlds as pending deletion...");
                    foreach (var w in oldWorlds)
                        w.IsPendingDeletion = true;
                    vaultContext.SaveChanges();

                    foreach (var w in oldWorlds)
                        DeleteWorld(vaultContext, w);
                }
            }

            vaultContext.Dispose();
            

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }

        private static List<String> FetchAvailableWorlds(String tldHostname)
        {
            var activeWorldRegex = new Regex("\"https?\\:\\/\\/([^\"]+)\"");
            var hostActiveWorlds = new Dictionary<String, List<String>>();

            Console.WriteLine("Fetching active servers for {0}...", tldHostname);
            var serverListUrl = $"http://www.{tldHostname}/backend/get_servers.php";
            var response = httpClient.GetStringAsync(serverListUrl).Result;
            var activeWorldMatches = activeWorldRegex.Matches(response);
            return activeWorldMatches.Select(m => m.Groups[1].Value).Where(w => !w.StartsWith("www")).ToList();
        }

        private static void FetchWorldData(VaultContext context, List<World> worlds, Dictionary<String, LocaleSettings> hostLocales, String hostname, bool overwrite)
        {
            var host = HostFor(hostname);

            var localeSettings = hostLocales.ContainsKey(host) ? hostLocales[host] : null;

            var fetchers = new IFetcher[]
            {
                new BuildingFetcher(),
                new ConfigFetcher()
                {
                    Overwrite = overwrite,
                    DefaultTimeZoneId = localeSettings?.TimeZoneId ?? "Europe/London"
                },
                new UnitFetcher()
            };

            var world = worlds.Where(w => w.Hostname == hostname).SingleOrDefault();
            if (world == null)
            {
                short translationId;
                if (localeSettings != null) translationId = localeSettings.DefaultTranslationId;
                else
                {
                    translationId = 1;
                    Console.WriteLine($"Warning: No default translation could be found for {hostname}, defaulting to primary English translation.");
                }

                world = new World
                {
                    Name = hostname.Split('.')[0],
                    Hostname = hostname,
                    DefaultTranslationId = translationId
                };
                context.Add(world);
            }

            var pendingFetchers = fetchers.Where(f => f.NeedsUpdate(world)).ToList();
            if (pendingFetchers.Count > 0)
                Console.WriteLine("Pulling for world {0}...", world.Hostname);
            else
                Console.WriteLine("World {0} is up to date", world.Hostname);

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
                fetcher.Process(context, world, data);

                Console.WriteLine("Done.");
            }

            var worldSettings = context.WorldSettings.Where(s => s.WorldId == world.Id).First();
            String timezoneId;
            if (localeSettings != null) timezoneId = hostLocales[host].TimeZoneId;
            else
            {
                Console.WriteLine($"Warning: No timezone ID could be found for {hostname}, please manually enter a timezone ID for the server.");
                Console.WriteLine("An exhaustive list of Timezone IDs can be found at: https://nodatime.org/TimeZones");
                Console.WriteLine("(The default for .net and .co.uk is 'Europe/London'.)");

                do
                {
                    Console.Write("Timezone ID: ");
                    timezoneId = Console.ReadLine().Trim();

                    if (NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.ForId(timezoneId) == null)
                    {
                        Console.WriteLine("Invalid ID: " + timezoneId);
                        timezoneId = null;
                    }
                } while (timezoneId == null);

                
                hostLocales.Add(host, new LocaleSettings { DefaultTranslationId = world.DefaultTranslationId, TimeZoneId = timezoneId });
            }

            worldSettings.TimeZoneId = timezoneId;
            context.SaveChanges();
        }

        private static void DeleteWorld(VaultContext vaultContext, World world)
        {
            vaultContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            List<Type> typeJobs = new List<Type>
            {
                typeof(User),
                typeof(UserLog),
                typeof(Command),
                typeof(CurrentBuilding),
                typeof(CurrentVillage),
                typeof(CurrentVillageSupport),
                typeof(CurrentArmy),
                typeof(CommandArmy),
                typeof(CurrentPlayer),
                typeof(EnemyTribe),
                typeof(IgnoredReport),
                typeof(Report),
                typeof(ReportArmy),
                typeof(ReportBuilding),
                typeof(Transaction),
                typeof(Village),
                typeof(Player),
                typeof(Ally),
                typeof(Conquer),
                typeof(AccessGroup)
            };

            Console.WriteLine("Deleting non-trivial datatypes...");
            Console.WriteLine("Deleting UserUploadHistory entries...");
            vaultContext.UserUploadHistory.RemoveRange(vaultContext.UserUploadHistory.Where(h => h.U.WorldId == world.Id));
            vaultContext.SaveChanges();

            var numJobsDone = 0;
            String JobsProgressMessage() => $"Deleting data for {world.Hostname} (id={world.Id}) ({numJobsDone}/{typeJobs.Count} done)";

            using (var dataProgressBar = new ProgressBar(typeJobs.Count, JobsProgressMessage()))
            {
                foreach (var type in typeJobs)
                {
                    using (var jobProgressBar = dataProgressBar.Spawn(1, ""))
                    {
                        Console.Out.Flush();
                        Thread.Sleep(100);
                        DeleteForWorld(vaultContext, jobProgressBar, type, world.Id);
                    }

                    numJobsDone++;
                    dataProgressBar.Tick(JobsProgressMessage());
                }
            }

            Console.WriteLine("Deleting world settings...");
            if (world.WorldSettings != null)
            {
                vaultContext.Remove(world.WorldSettings);
                vaultContext.SaveChanges();
            }

            Console.WriteLine("Deleting world...");
            vaultContext.Remove(world);
            vaultContext.SaveChanges();

            Console.WriteLine("Deleted all data for {0}.", world.Hostname);

            vaultContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        private static void DeleteForWorld(VaultContext context, IProgressBar progressBar, Type type, int worldId)
        {
            var entityType = context.Model.FindEntityType(type);
            var table = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var primaryKeys = entityType.FindPrimaryKey().Properties.Select(p => p.Name).Where(n => n != "WorldId" && n != "AccessGroupId").ToList();
            if (primaryKeys.Count != 1)
            {
                Console.WriteLine("Unexpected number of primary keys for {0}, got {1} but expected 1", type.Name, primaryKeys.Count);
            }

            var primaryKey = primaryKeys.Single();
            var numTotal = 0;

            progressBar.Tick(0, $"Counting {type.Name} entities...");
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(1) FROM {schema}.{table} WHERE world_id = {worldId}";
                context.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    result.Read();
                    numTotal = result.GetInt32(0);
                }
            }

            if (numTotal == 0)
            {
                progressBar.Tick(progressBar.MaxTicks);
                return;
            }

            progressBar.Message = $"Deleting {numTotal} entries of type {type.Name} with single transaction...";
            progressBar.MaxTicks = numTotal;

            int numDeleted = context.Database.ExecuteSqlRaw($"DELETE FROM {schema}.{table} WHERE world_id = {worldId}");
            if (numDeleted != numTotal)
            {
                Console.WriteLine("Deletion failed for {0}, expected to delete {1} but deleted {2} instead", type.Name, numTotal, numDeleted);
            }

            progressBar.Tick(numTotal);
        }
    }
}
