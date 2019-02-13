using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var config = new ConfigurationBuilder()
                .ApplyVaultConfiguration()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

            PreloadAssemblies();
            host.Run();
        }

        private static void PreloadAssemblies()
        {
            var pendingAssemblies = new Queue<Assembly>();
            var visitedAssemblies = new List<String>();

            pendingAssemblies.Enqueue(Assembly.GetExecutingAssembly());

            while (pendingAssemblies.Count > 0)
            {
                var current = pendingAssemblies.Dequeue();
                var dependencies = current.GetReferencedAssemblies();

                var newAssemblies = dependencies.Where(d => !visitedAssemblies.Contains(d.FullName)).ToList();
                foreach (var name in newAssemblies)
                    pendingAssemblies.Enqueue(Assembly.Load(name));

                visitedAssemblies.AddRange(newAssemblies.Select(d => d.FullName));
            }
        }
    }
}
