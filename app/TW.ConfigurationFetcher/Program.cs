using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using TW.Vault.Lib.Scaffold;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using ShellProgressBar;
using System.Threading;

namespace TW.ConfigurationFetcher
{
    class LocaleSettings
    {
        public short DefaultTranslationId { get; set; }
        public String TimeZoneId { get; set; }

        public static LocaleSettings FromWorld(World world) => new LocaleSettings { DefaultTranslationId = world.DefaultTranslationId, TimeZoneId = world.WorldSettings.TimeZoneId };

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
        public static bool Confirm(String message, Config config)
        {
            bool? response = null;
            if (config.AcceptAll) response = true;
            while (!response.HasValue)
            {
                Console.Write(message + " [y/n] ");
                var r = Console.ReadLine().ToLower();
                if (r == "y") response = true;
                if (r == "n") response = false;
            }
            return response.Value;
        }

        public static String HostFor(String hostname) => String.Join('.', hostname.Split('.').Skip(1)).ToLower();
        public static String HostOf(World world) => HostFor(world.Hostname);

        private static HttpClient _httpClient = null;
        public static HttpClient httpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = HttpClientFactory.Create(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false, Proxy = null });
                return _httpClient;
            }
        }

        static List<T> AwaitManyWithProgressBar<T>(String label, List<Task<T>> tasks)
        {

            using (var pb = new ProgressBar(tasks.Count, label))
            {
                int NumComplete() => tasks.Count(t => t.IsCompleted);
                do
                {
                    Task.WaitAny(tasks.ToArray());
                    int numComplete = NumComplete();
                    pb.Tick(numComplete, $"{label} ({numComplete}/{pb.MaxTicks})");
                } while (tasks.Any(t => !t.IsCompleted));
                pb.Tick(tasks.Count);
            }

            return tasks.Select(t => t.Result).ToList();
        }

        static List<IJob> BuildUpdateTasks(Config config, List<WorldDescriptor> currentState, List<WorldDescriptor> targetState)
        {
            var currentStateByHash = currentState.GroupBy(s => s.EffectiveHash(config)).ToDictionary(g => g.Key, g => g.Single());
            var targetStateByHash = targetState.GroupBy(s => s.EffectiveHash(config)).ToDictionary(g => g.Key, g => g.Single());

            var toAdd = targetStateByHash.Where(kvp => !currentStateByHash.ContainsKey(kvp.Key)).Select(kvp => kvp.Value).ToList();
            var toRemove = currentStateByHash.Where(kvp => !targetStateByHash.ContainsKey(kvp.Key)).Select(kvp => kvp.Value).ToList();
            var toUpdate = currentStateByHash
                .Where(kvp => targetStateByHash.ContainsKey(kvp.Key))
                .Select(kvp =>
                {
                    var current = currentStateByHash[kvp.Key];
                    var target = targetStateByHash[kvp.Key];
                    return new { Current = current, Target = target, ChangedProperties = current.CompareTo(target) };
                })
                .Where(cmp => cmp.ChangedProperties.Any())
                .ToList();

            var jobs = new List<IJob>();
            jobs.AddRange(toRemove.Select(d => new WorldDeletionJob(d, targetState.SingleOrDefault(t => t.Hostname == d.Hostname))));
            jobs.AddRange(toAdd.Select(d => new WorldCreationJob(d)));
            jobs.AddRange(toUpdate.Select(c => new WorldSettingsChangeJob(c.Current, c.ChangedProperties)));

            return jobs;
        }

        static async Task<WorldSettings> FetchLatestSettings(String hostname, String defaultTimeZoneId)
        {
            var endpoint = $"https://{hostname}/interface.php?func=get_config";
            var rawResponse = await httpClient.GetAsync(endpoint).ConfigureAwait(false);
            if (!rawResponse.IsSuccessStatusCode) return null;

            var response = await rawResponse.Content.ReadAsStringAsync();

            var readerSettings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                DtdProcessing = DtdProcessing.Ignore
            };

            var doc = new XPathDocument(XmlReader.Create(new StringReader(response), readerSettings));
            var xml = new XmlParser(doc.CreateNavigator());

            return new WorldSettings
            {
                CanDemolishBuildings = xml.Get<bool>("/config/build/destroy"),
                AccountSittingEnabled = xml.Get<bool>("/config/sitter/allow"),
                ArchersEnabled = xml.Get<bool>("/config/game/archer"),
                BonusVillagesEnabled = xml.Get<int, bool>("/config/coord/bonus_villages", i => i > 0),
                ChurchesEnabled = xml.Get<bool>("/config/game/church"),
                // I think this one's right? There's no obvious XML property for flags
                FlagsEnabled = xml.Get<int, bool>("/config/game/event", i => i > 0),
                // No direct properties for noble loyalty drop range either
                NoblemanLoyaltyMin = 20,
                NoblemanLoyaltyMax = 35,
                GameSpeed = xml.Get<decimal>("/config/speed"),
                MaxNoblemanDistance = xml.Get<short>("/config/snob/max_dist"),
                // No direct properties for militia enabled?
                MilitiaEnabled = true,
                MillisecondsEnabled = xml.Get<bool>("/config/commands/millis_arrival"),
                // Yes XML has a typo, morale -> moral
                MoraleEnabled = xml.Get<int, bool>("/config/moral", i => i > 0),
                NightBonusEnabled = xml.Get<int, bool>("/config/night/active", i => i > 0),
                PaladinEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 0),
                PaladinSkillsEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 1),
                PaladinItemsEnabled = xml.Get<int, bool>("/config/game/knight_new_items", i => i > 0),
                UnitSpeed = xml.Get<decimal>("/config/unit_speed"),
                WatchtowerEnabled = xml.Get<int, bool>("/config/game/watchtower", i => i > 0),
                TimeZoneId = defaultTimeZoneId
            };
        }

        static List<String> FetchPendingHostnames(VaultContext context, Config config)
        {
            var existingHostnames = context.World.Where(w => !w.IsPendingDeletion && !w.IsBeta).Select(w => w.Hostname).ToList();
            var knownHostnames = existingHostnames.Concat(config.ExtraServers);
            var allTlds = config.ExtraTLDs.Concat(knownHostnames.Select(HostFor)).Select(s => s.ToLower()).Distinct().ToList();
            if (!config.FetchTldServers) allTlds.Clear();

            List<String> activeWorldHostnames;
            if (config.FetchTldServers)
            {
                activeWorldHostnames = AwaitManyWithProgressBar(
                    "Fetch list of active servers from TLD indexes",
                    allTlds.Select(async h => await FetchAvailableWorlds(h).ConfigureAwait(false)).ToList()
                ).SelectMany(s => s).ToList();
            }
            else
            {
                activeWorldHostnames = new List<string>();
            }

            var allWorldHostnames = activeWorldHostnames.Concat(knownHostnames).Select(s => s.ToLower()).Distinct().ToList();
            return allWorldHostnames;
        }

        static Dictionary<String, LocaleSettings> CollectLocaleTemplates(VaultContext context, Config config, List<String> targetHostnames)
        {
            var result = new Dictionary<String, LocaleSettings>();
            var tlds = targetHostnames.Select(HostFor).Select(h => h.ToLower()).Distinct().ToList();
            var worlds = context.World.ToList();
            foreach (var tld in tlds)
            {
                var samples = worlds.Where(w => HostFor(w.Hostname.ToLower()) == tld).ToList();
                // fetches all configs for worlds from the given tld and
                // selects the most common config
                var localeSettings = samples
                    .Select(LocaleSettings.FromWorld)
                    .GroupBy(s => s)
                    .Select(g => new { Settings = g.Key, Count = g.Count() })
                    .OrderByDescending(s => s.Count)
                    // new tlds won't have any samples
                    .FirstOrDefault()
                    ?.Settings;

                if (localeSettings == null)
                {
                    Console.WriteLine($"New TLD \"{tld}\" needs locale settings: TimeZoneId, DefaultTranslationId");

                    String timezoneId;
                    short defaultTranslationId = -1;

                    if (config.AcceptAll)
                    {
                        Console.WriteLine("AcceptAll is enabled, filling with defaults (TimeZoneId=Europe/London, DefaultTranslationId=1)");
                        timezoneId = "Europe/London";
                        defaultTranslationId = 1;
                    }
                    else
                    {
                        #region TimeZoneId Prompt

                        Console.WriteLine("An exhaustive list of Timezone IDs can be found at: https://nodatime.org/TimeZones");
                        Console.WriteLine("(The default for .net and .co.uk is 'Europe/London'.)");

                        do
                        {
                            Console.Write("\tTimezone ID: ");
                            timezoneId = Console.ReadLine().Trim();

                            bool succeeded = true;

                            try { succeeded = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.ForId(timezoneId) != null; }
                            catch { succeeded = false; }

                            if (!succeeded)
                            {
                                Console.WriteLine($"\tInvalid ID: {timezoneId} - An exhaustive list of Timezone IDs can be found at: https://nodatime.org/TimeZones");
                                timezoneId = null;
                            }
                        } while (timezoneId == null);
                        #endregion

                        #region DefaultTranslationId Prompt

                        Console.WriteLine("The default ID for English is '1'.");

                        do
                        {
                            do Console.Write("\tTranslation ID: ");
                            while (!short.TryParse(Console.ReadLine(), out defaultTranslationId));

                            var translation = context.TranslationRegistry
                                .Where(r => r.Id == defaultTranslationId)
                                .Include(nameof(TranslationRegistry.Language))
                                .FirstOrDefault();

                            if (translation == null)
                            {
                                Console.WriteLine("\tTranslation does not exist");
                                defaultTranslationId = -1;
                                continue;
                            }

                            if (!Confirm($"\t{translation.Language.Name}: {translation.Name} by {translation.Author}?", config))
                            {
                                defaultTranslationId = -1;
                                continue;
                            }

                        } while (defaultTranslationId < 0);

                        #endregion
                    }

                    localeSettings = new LocaleSettings
                    {
                        DefaultTranslationId = defaultTranslationId,
                        TimeZoneId = timezoneId
                    };
                }

                foreach (var hostname in targetHostnames.Where(h => HostFor(h) == tld))
                    result.Add(hostname, localeSettings);
            }
            return result;
        }

        static List<WorldDescriptor> GenerateTargetState(VaultContext context, Config config)
        {
            var pendingHostnames = FetchPendingHostnames(context, config);
            var localeTemplates = CollectLocaleTemplates(context, config, pendingHostnames);

            var sem = new SemaphoreSlim(4);
            var pendingHostnameSettingsTasks = pendingHostnames.Select(h => new { Hostname = h, SettingsTask = FetchLatestSettings(h, localeTemplates[h].TimeZoneId) }).ToList();
            AwaitManyWithProgressBar("Fetch server settings", pendingHostnameSettingsTasks.Select(p => p.SettingsTask).ToList());

            return pendingHostnameSettingsTasks.Where(t => t.SettingsTask.Result != null).Select(t => new WorldDescriptor {
                Hostname = t.Hostname,
                Settings = t.SettingsTask.Result,
                DefaultTranslationId = localeTemplates[t.Hostname].DefaultTranslationId
            }).ToList();
        }

        static List<WorldDescriptor> GenerateCurrentState(VaultContext context)
        {
            return context.World.Include(w => w.WorldSettings).ToList().Select(w => new WorldDescriptor
            {
                Hostname = w.Hostname,
                Settings = w.WorldSettings,
                DefaultTranslationId = w.DefaultTranslationId
            }).ToList();
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
                Console.WriteLine("Assessing current DB state...");
                var currentState = GenerateCurrentState(vaultContext);
                Console.WriteLine("Assessing target DB state...");
                var targetState = GenerateTargetState(vaultContext, config);
                var jobs = BuildUpdateTasks(config, currentState, targetState);



                Console.WriteLine("Job collection done. Summary:");
                if (jobs.Count > 0)
                {
                    var workByJobType = jobs.GroupBy(j => j.GetType());
                    foreach (var group in workByJobType)
                    {
                        Console.WriteLine($"\n{group.Key.Name}: {group.Count()} Tasks");
                        var lines = new List<String>();
                        foreach (var item in group.Select(j => j.WorkSummary))
                        {
                            var innerLines = item.Split("\n");
                            lines.Add("- " + innerLines[0]);
                            foreach (var line in innerLines.Skip(1))
                                lines.Add("  " + line);
                        }

                        foreach (var line in lines)
                            Console.WriteLine("  " + line);
                    }
                }
                else
                {
                    Console.WriteLine("Everything up to date");
                }

                Console.WriteLine();

                if (!config.AcceptAll)
                {
                    Console.WriteLine("Press Enter to confirm.");
                    Console.ReadLine();
                }

                foreach (var job in jobs)
                {
                    Console.WriteLine("Running " + job);
                    job.Run(vaultContext, config);
                }
            }

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }

        private static async Task<List<String>> FetchAvailableWorlds(String tldHostname)
        {
            var activeWorldRegex = new Regex("\"https?\\:\\/\\/([^\"]+)\"");
            var hostActiveWorlds = new Dictionary<String, List<String>>();

            //Console.WriteLine("Fetching active servers for {0}...", tldHostname);
            var serverListUrl = $"http://www.{tldHostname}/backend/get_servers.php";
            var response = await httpClient.GetStringAsync(serverListUrl);
            var activeWorldMatches = activeWorldRegex.Matches(response);
            return activeWorldMatches.Select(m => m.Groups[1].Value).Where(w => !w.StartsWith("www")).ToList();
        }
    }
}
