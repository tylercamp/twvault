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
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

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

                var envMinAuthLevel = Environment.GetEnvironmentVariable("TW_VAULT_MIN_PRIVELEGES");
                if (envMinAuthLevel != null)
                {
                    short minAuthLevel = short.Parse(envMinAuthLevel);
                    cfg.MinimumRequiredPriveleges = minAuthLevel;
                }
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
    }
}
