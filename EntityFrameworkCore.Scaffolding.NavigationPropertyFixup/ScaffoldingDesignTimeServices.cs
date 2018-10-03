using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace EntityFrameworkCore.Scaffolding.NavigationPropertyFixup
{
    public class ScaffoldingDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IScaffoldingModelFactory, FixedRelationalScaffoldingModelFactory>();

            Debugger.Break();
        }
    }
}
