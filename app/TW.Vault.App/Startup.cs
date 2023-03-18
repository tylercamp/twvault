using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TW.Vault.Lib.Scaffold;
using TW.Vault.Lib.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json.Converters;
using System.IO;
using Hosting = Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using TW.Vault.App;
using Serilog;

namespace TW.Vault.App
{
    public class Startup
    {
        private static ILogger logger = Log.ForContext<Startup>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigins", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                    //builder.AllowCredentials();
                });
            });

            services
                .AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter(new SnakeCaseNamingStrategy(), false));
                })
                .AddMvcOptions(opt =>
                {
                    opt.Filters.Add(new IPLoggingInterceptionAttribute());
                    opt.Filters.Add(new ExceptionInterceptionAttribute());
                });

            services
                .AddScoped<RequireAuthAttribute>()
                .AddSingleton<Hosting.IHostedService, Lib.Features.HighScoresService>();

            String connectionString = Configuration.GetConnectionString("Vault");
            services.AddDbContext<VaultContext>(options => options.UseNpgsql(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Build obfuscated vault.js and copy to script output path
            var asputil = new Lib.ASPUtil(env);
            if (asputil.UseProductionScripts)
            {
                logger.Information("In production mode or script obfuscation was force-enabled, preparing production-ready scripts...");

                var scriptsOutputPath = asputil.ObfuscationPathRoot;
                logger.Information("Writing prod scripts to {outputPath}", scriptsOutputPath);
                if (!Directory.Exists(scriptsOutputPath))
                {
                    logger.Debug("Directory does not exist, creating...");
                    Directory.CreateDirectory(scriptsOutputPath);
                }

                // Build and copy primary app script vault.js
                logger.Information("Compiling vault.js...");
                var primaryScriptTargetPath = Path.Join(scriptsOutputPath, "vault.js");
                
                var compiler = new Lib.Features.ScriptCompiler();
                compiler.InitCommonVars();
                compiler.DependencyResolver = name => File.ReadAllText(asputil.GetFilePath(name));

                var compiled = compiler.Compile("vault.js");

                logger.Information("Obfuscating vault.js...");
                var rawFilePath = Path.Combine(Path.GetTempPath(), "vault.in.js");
                var outputFilePath = Path.Combine(Path.GetTempPath(), "vault.out.js");

                try
                {
                    File.WriteAllText(rawFilePath, compiled);

                    if (!ScriptObfuscation.Run(rawFilePath, outputFilePath))
                        throw new Exception("Script obfuscation was enabled but obfuscation failed");

                    if (File.Exists(primaryScriptTargetPath))
                        File.Delete(primaryScriptTargetPath);

                    logger.Information("Obfuscation successful, copying from {outputPath} to {targetPath}", outputFilePath, primaryScriptTargetPath);
                    File.Copy(outputFilePath, primaryScriptTargetPath);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Script obfuscation failed, was 'javascript-obfuscator' installed with npm?");
                    throw;
                }
                finally
                {
                    File.Delete(rawFilePath);
                    File.Delete(outputFilePath);
                }

                // Also copy main.js for faster file serving
                logger.Information("Copying main.js...");
                var mainJsContents = compiler.Compile("main.js");
                var targetMainJsPath = Path.Combine(scriptsOutputPath, "main.js");
                if (File.Exists(targetMainJsPath))
                    File.Delete(targetMainJsPath);
                File.WriteAllText(targetMainJsPath, mainJsContents);
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            });

            app.UseVaultContentDecryption();

            app.UseRouting();

            app.UseCors("AllOrigins");

            app.UseEndpoints(e => e.MapControllers());
        }
    }
}
