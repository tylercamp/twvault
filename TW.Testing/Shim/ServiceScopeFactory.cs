﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public IServiceProvider ServiceProvider => new ServiceProvider();

        public void Dispose()
        {
            
        }
    }

    class ServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(Vault.Scaffold.VaultContext))
                return new Vault.Scaffold.VaultContext(
                    new DbContextOptionsBuilder<Vault.Scaffold.VaultContext>()
                        .UseNpgsql("Server=v.tylercamp.me; Port=22342; Database=vault_dev; User Id=twu_vault; Password=!!TWV@ult4Us??")
                        .Options
                );
            else
                return null;
        }
    }
}
