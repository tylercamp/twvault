using System;
using System.IO;
using System.Net.Http;
using TW.ConfigurationFetcher.Fetcher;

namespace TW.ConfigurationFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var worlds = args;

#if DEBUG
            if (worlds.Length == 0)
                worlds = new[] { "en100.tribalwars.net" };
#endif

            if (worlds.Length == 0)
            {
                Console.WriteLine("No server worlds specified");
                return;
            }

            var fetchers = new IFetcher[]
            {
                new BuildingFetcher(),
                new ConfigFetcher(),
                new UnitFetcher()
            };

            var httpClient = new HttpClient();

            foreach (var world in worlds)
            {
                var baseUrl = $"https://{world}";

                foreach (var fetcher in fetchers)
                {
                    var url = $"{baseUrl}{fetcher.Endpoint}";
                    Console.Write("Fetching {0} ... ", url);
                    var response = httpClient.GetAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("ERROR: Request failed with code {0}", response.StatusCode);
                        continue;
                    }

                    Console.Write("Processing... ");

                    var data = response.Content.ReadAsStringAsync().Result;
                    fetcher.Process(world, data);

                    Console.WriteLine("Done.");
                }
            }

            Console.WriteLine("FINISHED.");
            Console.ReadLine();
        }
    }
}
