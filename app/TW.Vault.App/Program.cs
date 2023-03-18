using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using TW.Vault.Lib;

namespace TW.Vault.App
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

            var builder = WebApplication.CreateBuilder(args);
            // https://learn.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-7.0&tabs=visual-studio#use-startup-with-the-new-minimal-hosting-model
            var startup = new Startup(config);

            startup.ConfigureServices(builder.Services);
            builder.Host.UseSerilog();

            builder.WebHost
                .UseKestrel()
                .UseConfiguration(config);

            var app = builder.Build();
            startup.Configure(app, app.Environment);

            PreloadAssemblies();
            app.Run();
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
