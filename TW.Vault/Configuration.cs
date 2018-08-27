using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
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

        public static SecurityConfiguration Security
        {
            get
            {
                SecurityConfiguration cfg = new SecurityConfiguration();
                Instance.GetSection("Security").Bind(cfg);
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
    }

    public class SecurityConfiguration
    {
        public bool AllowUploadArmyForNonOwner { get; set; }
        public bool ReportIgnoreExpectedPopulationBounds { get; set; }
        public bool AllowCommandArrivalBeforeServerTime { get; set; }

        public short MinimumRequiredPriveleges { get; set; }

        public bool EnableScriptFilter { get; set; }
        public List<String> PublicScripts { get; set; }
    }

    public class InitializationConfiguration
    {
        public bool EnableRequiredFiles { get; set; }
        public List<String> RequiredFiles { get; set; }
    }
}
