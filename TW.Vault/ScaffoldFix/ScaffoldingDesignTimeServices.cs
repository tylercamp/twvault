using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Scaffolding.NavigationPropertyFixup
{
    public class ScaffoldingDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            Console.WriteLine("Waiting...");
            while (!Debugger.IsAttached)
                Task.Delay(500).Wait();

            Debugger.Break();

            serviceCollection.AddSingleton<IScaffoldingModelFactory, FixedRelationalScaffoldingModelFactory>();
            serviceCollection.AddSingleton<ICandidateNamingService, FixedCandidateNamingService>();
        }
    }
}
