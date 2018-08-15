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

            var envPort = Environment.GetEnvironmentVariable("TWVAULT_PORT");

            if (envPort != null)
            {
                int port = int.Parse(envPort);
                config["urls"] = $"http://*:{port}";
            }

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

            host.Run();
        }
           
    }
}
