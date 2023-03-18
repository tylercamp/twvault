using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using TW.Vault.Lib;

namespace TW.Vault.Manage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .ApplyVaultConfiguration()
                .Build();

            Configuration.Require("UseCaptcha");

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

            app.Run();
        }
    }
}
