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
using Twilio;

namespace TW.Vault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(8, 4);

            var config = new ConfigurationBuilder()
                .ApplyVaultConfiguration()
                .Build();

            TwilioClient.Init(
                Configuration.Behavior.Notifications.TwilioClientKey,
                Configuration.Behavior.Notifications.TwilioClientSecret
            );

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
