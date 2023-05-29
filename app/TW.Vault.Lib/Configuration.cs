using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib
{
    public static class Configuration
    {
        private static IConfigurationRoot cachedRoot;
        private static DateTime rootCachedAt = new DateTime();
        private static TimeSpan configurationRefreshRate = TimeSpan.FromSeconds(10);

        public static IConfigurationRoot Instance
        {
            get
            {
                var now = DateTime.Now;
                if (now - rootCachedAt >= configurationRefreshRate)
                {
                    var builder = new ConfigurationBuilder().ApplyVaultConfiguration();

                    cachedRoot = builder.Build();
                    rootCachedAt = now;
                }

                return cachedRoot;
            }
        }

        public static void Require(params String[] keys)
        {
            var missing = keys.Where(k => Instance[k] == null || Instance[k].Trim().Length == 0).ToList();
            if (missing.Any())
            {
                var summary = String.Join("\n", missing.Select(k => $"- {k}"));
                var msg = $"Required configuration values are missing:\n{summary}";
                throw new Exception(msg);
            }
        }

        public static String ConnectionString
        {
            get
            {
                var connectionStrings = Instance.GetSection("ConnectionStrings");
                var directConnectionString = connectionStrings.GetValue<String>("Vault");
                if (directConnectionString != null)
                    return directConnectionString;

                var connectionOptions = connectionStrings.GetSection("Vault");

                var paramParts = new Dictionary<String, String>
                {
                    { "Server", connectionOptions["Server"] },
                    { "Port", connectionOptions["Port"] },
                    { "Database", connectionOptions["Database"] },
                    { "User Id", connectionOptions["User"] },
                    { "Password", connectionOptions["Password"] }
                };

                return String.Join("; ", paramParts.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }
        }

        public static SecurityConfiguration Security
        {
            get
            {
                SecurityConfiguration cfg = new SecurityConfiguration();
                Instance.GetSection("Security").Bind(cfg);
                cfg.Encryption = new EncryptionConfiguration();
                Instance.GetSection("Security").GetSection("Encryption").Bind(cfg.Encryption);
                return cfg;
            }
        }

        public static InitializationConfiguration Initialization
        {
            get
            {
                InitializationConfiguration cfg = new InitializationConfiguration();
                Instance.GetSection("Initialization").Bind(cfg);
                return cfg;
            }
        }

        public static BehaviorConfiguration Behavior
        {
            get
            {
                BehaviorConfiguration cfg = new BehaviorConfiguration();
                cfg.Map = new MapBehaviorConfiguration();
                Instance.GetSection("Behavior").Bind(cfg);
                Instance.GetSection("Behavior:Map").Bind(cfg.Map);
                Instance.GetSection("Behavior:Tagging").Bind(cfg.Tagging);
                return cfg;
            }
        }

        public static RankingsConfiguration Rankings
        {
            get
            {
                RankingsConfiguration cfg = new RankingsConfiguration();
                Instance.GetSection("Rankings").Bind(cfg);
                return cfg;
            }
        }

        public static TranslationConfiguration Translation
        {
            get
            {
                TranslationConfiguration cfg = new TranslationConfiguration();
                Instance.GetSection("Translation").Bind(cfg);
                return cfg;
            }
        }
    }

    public class SecurityConfiguration
    {
        public bool AllowUploadArmyForNonOwner { get; set; }
        public bool ReportIgnoreExpectedPopulationBounds { get; set; }
        public bool AllowCommandArrivalBeforeServerTime { get; set; }
        public bool RestrictSitterAccess { get; set; }
        public bool RestrictAccessWithinTribes { get; set; }

        public short MinimumRequiredPriveleges { get; set; }

        public EncryptionConfiguration Encryption { get; set; }

        public bool ForceEnableObfuscatedScripts { get; set; }

        public Guid? ForcedKey { get; set; }
        public long? ForcedPlayerId { get; set; }
        public long? ForcedTribeId { get; set; }
    }

    public class EncryptionConfiguration
    {
        public bool UseEncryption { get; set; } = true;
        public uint SeedSalt { get; set; } = 0x8D760E23;
        public uint SeedPrime { get; set; } = 2035567511;
    }

    public class BehaviorConfiguration
    {
        public MapBehaviorConfiguration Map { get; set; }
        public TaggingBehaviorConfiguration Tagging { get; set; }

        public bool DisableFakeScript { get; set; } = false;
        public bool DisableAutoTagger { get; set; } = false;
    }

    public class MapBehaviorConfiguration
    {
        public int MaxDaysSinceReportUpload { get; set; } = 1;
        public int MaxDaysSinceTroopUpload { get; set; } = 3;
        public int MaxDaysSinceCommandUpload { get; set; } = 3;
        public int MaxDaysSinceIncomingsUpload { get; set; } = 3;
    }

    public class TaggingBehaviorConfiguration
    {
        public int MaxDaysSinceReportUpload { get; set; } = 1;
        public int MaxDaysSinceTroopUpload { get; set; } = 3;
        public int MaxDaysSinceCommandUpload { get; set; } = 3;
        public int MaxDaysSinceIncomingsUpload { get; set; } = 3;
    }
    
    public class InitializationConfiguration
    {
        public String ScriptCompilationOutputPath { get; set; } = "obfuscated";
        public String ServerHostname { get; set; } = "v.tylercamp.me";
        public String ServerBasePath { get; set; } = "";
    }

    public class RankingsConfiguration
    {
        public bool EnableRankingsService { get; set; } = true;
        public int RefreshCheckIntervalSeconds { get; set; } = 300;
    }

    public class TranslationConfiguration
    {
        public short BaseTranslationId { get; set; }
    }
}
