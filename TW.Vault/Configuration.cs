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
    }
}
