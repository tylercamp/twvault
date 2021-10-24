using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TW.Vault.Lib.Scaffold;

namespace TW.ConfigurationFetcher
{
    abstract class IJob
    {
        public abstract void Run(VaultContext context, Config config);
        public abstract String WorkSummary { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}({WorkSummary})";
        }
    }

    class WorldDeletionJob : IJob
    {
        WorldDescriptor worldDescriptor, replacement;
        public WorldDeletionJob(WorldDescriptor worldDescriptor, WorldDescriptor replacement)
        {
            this.worldDescriptor = worldDescriptor;
            this.replacement = replacement;
        }

        public override String WorkSummary
        {
            get
            {
                var result = worldDescriptor.Hostname;
                if (replacement != null)
                {
                    var diff = worldDescriptor.CompareTo(replacement);
                    result += "\n" + String.Join("\n", diff.Select(d => $"- {d}"));
                }
                return result;
            }
        }

        public override void Run(VaultContext vaultContext, Config config)
        {
            if (!config.AcceptAll)
            {
                Console.Write("Preparing to delete... (press Enter)");
                Console.ReadLine();
            }

            var world = vaultContext.World.Where(w => w.Hostname == worldDescriptor.Hostname).Single();

            vaultContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            List<Type> typeJobs = new List<Type>
            {
                typeof(User),
                typeof(UserLog),
                typeof(Command),
                typeof(CurrentBuilding),
                typeof(CurrentVillage),
                typeof(CurrentVillageSupport),
                typeof(CurrentArmy),
                typeof(CommandArmy),
                typeof(CurrentPlayer),
                typeof(EnemyTribe),
                typeof(IgnoredReport),
                typeof(Report),
                typeof(ReportArmy),
                typeof(ReportBuilding),
                typeof(Transaction),
                typeof(Village),
                typeof(Player),
                typeof(Ally),
                typeof(Conquer),
                typeof(AccessGroup)
            };

            Console.WriteLine("Deleting non-trivial datatypes...");
            Console.WriteLine("Deleting UserUploadHistory entries...");
            vaultContext.UserUploadHistory.RemoveRange(vaultContext.UserUploadHistory.Where(h => h.U.WorldId == world.Id));
            vaultContext.SaveChanges();

            var numJobsDone = 0;
            String JobsProgressMessage() => $"Deleting data for {world.Hostname} (id={world.Id}) ({numJobsDone}/{typeJobs.Count} done)";

            using (var dataProgressBar = new ProgressBar(typeJobs.Count, JobsProgressMessage()))
            {
                foreach (var type in typeJobs)
                {
                    using (var jobProgressBar = dataProgressBar.Spawn(1, ""))
                    {
                        Console.Out.Flush();
                        Thread.Sleep(100);
                        DeleteForWorld(vaultContext, jobProgressBar, type, world.Id);
                    }

                    numJobsDone++;
                    dataProgressBar.Tick(JobsProgressMessage());
                }
            }

            Console.WriteLine("Deleting world settings...");
            if (world.WorldSettings != null)
            {
                vaultContext.Remove(world.WorldSettings);
                vaultContext.SaveChanges();
            }

            Console.WriteLine("Deleting world...");
            vaultContext.Remove(world);
            vaultContext.SaveChanges();

            Console.WriteLine("Deleted all data for {0}.", world.Hostname);

            vaultContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        private void DeleteForWorld(VaultContext context, IProgressBar progressBar, Type type, int worldId)
        {
            var entityType = context.Model.FindEntityType(type);
            var table = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var primaryKeys = entityType.FindPrimaryKey().Properties.Select(p => p.Name).Where(n => n != "WorldId" && n != "AccessGroupId").ToList();
            if (primaryKeys.Count != 1)
            {
                Console.WriteLine("Unexpected number of primary keys for {0}, got {1} but expected 1", type.Name, primaryKeys.Count);
            }

            var primaryKey = primaryKeys.Single();
            var numTotal = 0;

            progressBar.Tick(0, $"Counting {type.Name} entities...");
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(1) FROM {schema}.{table} WHERE world_id = {worldId}";
                context.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    result.Read();
                    numTotal = result.GetInt32(0);
                }
            }

            if (numTotal == 0)
            {
                progressBar.Tick(progressBar.MaxTicks);
                return;
            }

            progressBar.Message = $"Deleting {numTotal} entries of type {type.Name} with single transaction...";
            progressBar.MaxTicks = numTotal;

            using (var tx = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET session_replication_role=replica");
                int numDeleted = context.Database.ExecuteSqlRaw($"DELETE FROM {schema}.{table} WHERE world_id = {worldId}");
                if (numDeleted != numTotal)
                {
                    Console.WriteLine("Deletion failed for {0}, expected to delete {1} but deleted {2} instead", type.Name, numTotal, numDeleted);
                }
                context.Database.ExecuteSqlRaw("SET session_replication_role = DEFAULT");
                tx.Commit();
            }

            progressBar.Tick(numTotal);
        }
    }

    class WorldCreationJob : IJob
    {
        WorldDescriptor descriptor;
        public WorldCreationJob(WorldDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public override String WorkSummary => descriptor.Hostname;

        public override void Run(VaultContext context, Config config)
        {
            var newWorld = new World
            {
                Name = descriptor.Hostname.Split('.')[0],
                Hostname = descriptor.Hostname,
                DefaultTranslationId = descriptor.DefaultTranslationId,
                IsBeta = false,
                IsPendingDeletion = false
            };

            newWorld.WorldSettings = descriptor.Settings;

            context.Add(descriptor.Settings);
            context.Add(newWorld);

            context.SaveChanges();
        }
    }

    class WorldSettingsChangeJob : IJob
    {
        WorldDescriptor worldDescriptor;
        List<PropertyDiff> changedProperties;

        public WorldSettingsChangeJob(WorldDescriptor worldDescriptor, List<PropertyDiff> changedProperties)
        {
            this.worldDescriptor = worldDescriptor;
            this.changedProperties = changedProperties;
        }

        public override string WorkSummary => $"{worldDescriptor.Hostname} change {String.Join(", ", changedProperties)}";

        public override void Run(VaultContext context, Config config)
        {
            var world = context.World.Include(w => w.WorldSettings).Where(w => w.Hostname == worldDescriptor.Hostname).Single();
            var settings = world.WorldSettings;
            foreach (var change in changedProperties)
            {
                if (change.Property.GetValue(settings).ToString() != change.OldValue.ToString())
                {
                    // indicates a bug in diffing logic, might be modifying the wrong WorldSettings
                    // or the comparison of new<->old might be backwards
                    Console.WriteLine($"WARNING: Diff of property value is invalid (expected old={change.OldValue}, got {change.Property.GetValue(settings)})");
                    Console.ReadLine();
                }

                change.Property.SetValue(settings, change.NewValue);
            }

            context.Update(settings);
            context.SaveChanges();
        }
    }
}
