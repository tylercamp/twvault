using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TW.ConfigurationFetcher
{
    class Config
    {
        public String ConnectionString { get; private set; }
        public bool FetchTldServers { get; private set; }
        public bool Clean { get; private set; }
        public List<String> ExtraTLDs { get; private set; } = new List<string>();
        public List<String> ExtraServers { get; private set; } = new List<string>();
        public bool FetchExisting { get; private set; }

        public bool IsValid => ConnectionString != null;

        private static Dictionary<String, String> ParseConnectionString(String connectionString)
        {
            return connectionString.Split(";").Select(s => s.Trim()).ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);
        }

        public override string ToString()
        {
            var cs = ParseConnectionString(ConnectionString);

            String GetOrElse(String key, String defaultValue) => cs.ContainsKey(key) ? cs[key] : defaultValue;

            var csSummary = String.Join("; ", new string[]
            {
                $"Server={GetOrElse("Server", "<None>")}",
                $"Database={GetOrElse("Database", "<None>")}",
                $"User Id={GetOrElse("User Id", "<None>")}"
            });

            return String.Join("\n", new string[]
            {
                $"- ConnectionString: {csSummary}",
                $"- Fetch TLD Servers: {FetchTldServers}",
                $"- Clean: {Clean}",
                $"- ExtraTLDs: " + (ExtraTLDs.Count == 0 ? "<None>" : String.Join(", ", ExtraTLDs)),
                $"- ExtraServers: " + (ExtraServers.Count == 0 ? "<None>" : String.Join(", ", ExtraServers)),
                $"- Fetch Old: {FetchExisting}"
            });
        }

        public static Config ParseParams(String[] args)
        {
            var result = new Config();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToLower();
                bool isLast = i == args.Length - 1;
                if (i == 0)
                {
                    result.ConnectionString = args[i].Trim('"', '\'');
                }
                else if (arg == "-extratld" && !isLast)
                {
                    ++i;
                    result.ExtraTLDs.Add(args[i].ToLower());
                }
                else if (arg == "-extraserver" && !isLast)
                {
                    ++i;
                    result.ExtraServers.Add(args[i].ToLower());
                }
                else if (arg == "-clean")
                {
                    result.Clean = true;
                }
                else if (arg == "-fetch-all")
                {
                    result.FetchTldServers = true;
                }
                else if (arg == "-fetch-old")
                {
                    result.FetchExisting = true;
                }
            }

            return result;
        }

        public static String UsageDescription => @"
Usage:

    > TW.ConfigurationFetcher ""<connection-string>"" <options...>

Options:

    <connection-string> : The full Postgres connection string for the Vault database.

                          Example: > TW.ConfigurationFetcher ""Server=127.0.0.1; Port=5432; Database=vault; User Id=u_vault; Password=vaulttest""

    -clean       : If specified, will check for old, closed servers and automatically delete their data.

    -fetch-all   : If specified, will gather all TLD servers (eg tribalwars.net, tribalwars.us, ...) and will
                   automatically register all game servers discovered.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-all

                        If ""en100.tribalwars.net"" is already registered, ""tribalwars.net"" would be used
                        to also find and register ""en101.tribalwars.net"", etc..

    -fetch-old   : If specified, will pull configuration for worlds that are already registered and will
                   update their configuration if necessary.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-old
                   
                        If ""en100.tribalwars.net"" is already registered, settings will be pulled from TW and
                        compared to the stored settings for that server. If they don't match, a warning will be
                        logged showing the change and will wait for confirmation before overwriting.

    -extraserver : Specifies an extra game server (eg en100.tribalwars.net) to be registered, if not done
                   already. Does not affect ""-fetch-all"".

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -extraserver en100.tribalwars.net -extraserver us40.tribalwars.us

    -extraTLD    : Specifies an extra top-level domain of a server (eg tribalwars.net, tribalwars.co.uk)
                   to be used when fetching all available servers. Has no effect when -fetch-all is not used.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-all -extraTLD tribalwars.net -extraTLD tribalwars.us

".Trim();
    }
}
