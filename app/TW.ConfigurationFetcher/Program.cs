using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using TW.ConfigurationFetcher.Fetcher;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Connection string must be provided as first argument");
                Environment.ExitCode = 1;
                return;
            }

            var connectionString = args[0];
            var worlds = args.Skip(1).Select(a => a.ToLower()).ToArray();

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

            var httpClient = new HttpClient();

            using (vaultContext)
            using (httpClient)
            {
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

                    var baseUrl = $"https://{hostname}";

                    foreach (var fetcher in fetchers)
                    {
                        var url = $"{baseUrl}{fetcher.Endpoint}";
                        Console.Write("Fetching {0} ... ", url);
                        var response = httpClient.GetAsync(url).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("ERROR: Request failed with code {0}", response.StatusCode);
                            Environment.ExitCode = 1;
                            continue;
                        }

                        Console.Write("Processing... ");

                        var data = response.Content.ReadAsStringAsync().Result;
                        fetcher.Process(vaultContext, world, data);

                        Console.WriteLine("Done.");
                    }
                }
            }

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }
    }
}
