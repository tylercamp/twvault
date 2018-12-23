using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TW.Testing.Shim
{
    class ServiceScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            return new ServiceScope();
        }
    }

    class ServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; } = new ServiceProvider();

        public void Dispose()
        {
            
        }
    }

    class ServiceProvider : IServiceProvider
    {
        static SerilogLoggerFactory loggerFactory;
        static DbContextOptions<Vault.Scaffold.VaultContext> dbOptions;

        public ServiceProvider()
        {
            if (loggerFactory == null)
                loggerFactory = new SerilogLoggerFactory();

            if (dbOptions == null)
                dbOptions = new DbContextOptionsBuilder<Vault.Scaffold.VaultContext>()
                            .UseNpgsql(Vault.Configuration.ConnectionString)
                            .UseLoggerFactory(loggerFactory)
                            .Options;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(Vault.Scaffold.VaultContext))
                return new Vault.Scaffold.VaultContext(dbOptions);
            else
                return null;
        }
    }
}
