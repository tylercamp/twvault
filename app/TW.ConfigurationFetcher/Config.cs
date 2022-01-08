using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TW.Vault.Lib.Scaffold;

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
        public bool AcceptAll { get; private set; }
        public List<PropertyInfo> ResetOnDiff { get; private set; } = new List<PropertyInfo>();

        public bool IsValid => ConnectionString != null;

        public Config()
        {
            var defaultResetProps = new List<String> { "GameSpeed", "UnitSpeed", "ArchersEnabled", "MoraleEnabled" };
            ResetOnDiff = defaultResetProps.Select(ConvertNameToProperty).Where(p => p != null).ToList();
        }

        private static PropertyInfo ConvertNameToProperty(String name)
        {
            var type = typeof(WorldSettings);
            var props = type.GetProperties();
            var match = props.SingleOrDefault(p => p.Name.ToLower() == name.ToLower());
            if (match == null)
                Console.WriteLine($"Warning: WorldSettings property '{name}' does not exist");

            return match;
        }

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
                else switch (arg)
                {
                    case "-extratld" when !isLast:
                        ++i;
                        result.ExtraTLDs.Add(args[i].ToLower());
                        break;

                    case "-extraserver" when !isLast:
                        ++i;
                        result.ExtraServers.Add(args[i].ToLower());
                        break;

                    case "-clean": result.Clean = true; break;
                    case "-fetch-all": result.FetchTldServers = true; break;
                    case "-fetch-old": result.FetchExisting = true; break;
                    case "-accept": result.AcceptAll = true; break;
                    case "-reset-on-diff" when !isLast:
                        ++i;
                        result.ResetOnDiff = args[i].ToLower().Split(",").Select(f => f.Trim()).Where(f => f.Length > 0).Select(ConvertNameToProperty).Where(p => p != null).ToList();
                        break;
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

    -extraserver : Specifies an extra game server (eg en100.tribalwars.net) to be registered, if not done
                   already. Does not affect ""-fetch-all"".

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -extraserver en100.tribalwars.net -extraserver us40.tribalwars.us

    -clean       : If specified, will check for old, closed servers and automatically delete their data.

    -fetch-all   : If specified, will gather all TLD servers (eg tribalwars.net, tribalwars.us, ...) and will
                   automatically register all game servers discovered.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-all

                        If ""en100.tribalwars.net"" is already registered, ""tribalwars.net"" would be used
                        to also find and register ""en101.tribalwars.net"", etc..

    -extraTLD    : Specifies an extra top-level domain of a server (eg tribalwars.net, tribalwars.co.uk)
                   to be used when fetching all available servers. Has no effect when -fetch-all is not used.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-all -extraTLD tribalwars.net -extraTLD tribalwars.us

    -fetch-old   : If specified, will pull configuration for worlds that are already registered and will
                   update their configuration if necessary.

                   Example: > TW.ConfigurationFetcher ""<connection-string>"" -fetch-old
                   
                        If ""en100.tribalwars.net"" is already registered, settings will be pulled from TW and
                        compared to the stored settings for that server. If they don't match, a warning will be
                        logged showing the change and will wait for confirmation before overwriting.

    -reset-on-diff : If specified with -fetch-old, deletes and recreates any worlds whose config differ
                     on the given property. Specify a property with ""-reset-on-diff a,b"". The default value
                     is ""GameSpeed,UnitSpeed,ArchersEnabled,MoraleEnabled""

    -accept      : If specified with -fetch-old, any prompts to modify old worlds will be accepted.

".Trim();
    }
}
