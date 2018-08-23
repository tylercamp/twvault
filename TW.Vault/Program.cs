using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TW.Vault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(8, 4);

            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{currentEnv}.json", optional: true)
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            ApplyEnvironmentConfig(config);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            foreach (var log in PendingConfigLogs)
                Log.ForContext<Program>().Information($"Environment config log: {log}");

            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

            host.Run();
        }

        static void ApplyEnvironmentConfig(IConfigurationRoot config)
        {
            var envPort = Environment.GetEnvironmentVariable("TWVAULT_PORT");
            if (envPort != null)
            {
                PendingConfigLogs.Add($"Overriding port from env to: {envPort}");
                int port = int.Parse(envPort);
                config["urls"] = $"http://*:{port}";
            }

            var connectionString = Environment.GetEnvironmentVariable("TWVAULT_CONNECTION_STRING");
            if (connectionString != null)
            {
                PendingConfigLogs.Add($"Overriding connection string to: {connectionString}");
                config["ConnectionStrings:Vault"] = connectionString;
            }

            var envMinAuthLevel = Environment.GetEnvironmentVariable("TWVAULT_MIN_PRIVILEGES");
            if (envMinAuthLevel != null)
            {
                PendingConfigLogs.Add($"Overriding min vault privileges to: {((Security.PermissionLevel)short.Parse(envMinAuthLevel)).ToString()}");
                config["Security:MinimumRequiredPriveleges"] = envMinAuthLevel;
            }

            var logLocation = Environment.GetEnvironmentVariable("TWVAULT_LOG_LOCATION");
            if (logLocation != null)
            {
                PendingConfigLogs.Add($"Overriding log location format to: '{logLocation}'");
                config["Serilog:WriteTo:2:Args:configure:0:Args:pathFormat"] = logLocation;
            }
        }

        static List<String> PendingConfigLogs = new List<String>();
           
    }
}
