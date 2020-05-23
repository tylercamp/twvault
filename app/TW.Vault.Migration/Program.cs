using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using TW.Vault.Scaffold;

namespace TW.Vault.Migration
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Connection string not provided");
                return 1;
            }

            var connectionString = args[0];
            Console.WriteLine("Applying migrations using connectionString = [{0}]", connectionString);

            var context = new VaultContext(
                new DbContextOptionsBuilder<VaultContext>()
                    .UseNpgsql(connectionString, x => x.MigrationsAssembly("TW.Vault.Migration"))
                    .Options
            );

            var migrations = context.Database.GetPendingMigrations().ToList();
            Console.WriteLine("Applying {0} pending migrations:", migrations.Count);
            foreach (var m in migrations)
                Console.WriteLine(m);

            context.Database.Migrate();

            return 0;
        }
    }
}