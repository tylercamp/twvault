using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TW.Vault.Scaffold;
using TW.Vault.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json.Converters;
using System.IO;
using Hosting = Microsoft.Extensions.Hosting;

namespace TW.Vault
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter(camelCaseText: false));
                })
                .AddMvcOptions(opt =>
                {
                    opt.Filters.Add(new IPLoggingInterceptionAttribute());
                });

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
                .AddScoped<RequireAuthAttribute>()
                .AddSingleton<Hosting.IHostedService, Features.HighScoresService>();

            String connectionString = Configuration.GetConnectionString("Vault");
            services.AddDbContext<VaultContext>(options => options.UseNpgsql(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var initCfg = TW.Vault.Configuration.Initialization;
            if (initCfg.EnableRequiredFiles)
            {
                var webRoot = env.WebRootPath;
                foreach (var file in initCfg.RequiredFiles)
                {
                    String fullPath = Path.Combine(webRoot, file);
                    if (!File.Exists(fullPath))
                        throw new Exception($"Required external script is not available: \"{file}\" (relative to \"{webRoot}\") (absolute path \"${Path.GetFullPath(fullPath)}\")");
                }
            }

            app.UseCors("AllOrigins");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            });

            app.UseVaultContentDecryption();
            
            app.UseMvc();
        }
    }
}
