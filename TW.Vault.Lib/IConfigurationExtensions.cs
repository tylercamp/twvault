using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class IConfigurationExtensions
    {
        public static IConfigurationBuilder ApplyVaultConfiguration(this IConfigurationBuilder builder)
        {
            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .AddJsonFile($"appsettings.{currentEnv}.json", optional: true)
                   .AddJsonFile("hosting.json", optional: true)
                   .AddEnvironmentVariables();
        }
    }
}
