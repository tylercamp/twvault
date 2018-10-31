using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.Features
{
    public static class Profiling
    {
        private static ConcurrentDictionary<String, ConcurrentBag<TimeSpan>> PerformanceRecords = new ConcurrentDictionary<string, ConcurrentBag<TimeSpan>>();

        private static TimeSpan PersistProfilingInterval = TimeSpan.FromMinutes(float.Parse(Configuration.Instance["ProfilingStoreIntervalMinutes"] ?? "30.0"));
        private static DateTime LastPersistedAt = DateTime.UtcNow;

        public static int NumPendingRecords => PerformanceRecords.Sum(r => r.Value.Count);

        public static void AddRecord(String label, TimeSpan duration)
        {
            ConcurrentBag<TimeSpan> currentRecords = PerformanceRecords.GetOrAdd(label, (k) => new ConcurrentBag<TimeSpan>());
            currentRecords.Add(duration);
        }

        public static async Task StoreData(Scaffold.VaultContext context, bool force = false, bool rollover = true)
        {
            DateTime now = DateTime.UtcNow;
            if (force || now - LastPersistedAt > PersistProfilingInterval)
            {
                var records = Interlocked.Exchange(ref PerformanceRecords, new ConcurrentDictionary<String, ConcurrentBag<TimeSpan>>());
                if (records.IsEmpty)
                    return;

                var newRecords = new List<Scaffold.PerformanceRecord>();

                foreach (var kvp in records)
                {
                    var operation = kvp.Key;
                    var samples = kvp.Value;

                    if (samples.Count < 10 && rollover)
                    {
                        foreach (var time in kvp.Value)
                            AddRecord(kvp.Key, time);
                        continue;
                    }

                    var newRecord = new Scaffold.PerformanceRecord();
                    newRecord.GeneratedAt = now;
                    newRecord.NumSamples = samples.Count;
                    newRecord.OperationLabel = operation;

                    newRecord.AverageTime = TimeSpan.FromSeconds(samples.Average(s => s.TotalSeconds));
                    newRecord.MinTime = TimeSpan.FromSeconds(samples.Min(s => s.TotalSeconds));
                    newRecord.MaxTime = TimeSpan.FromSeconds(samples.Max(s => s.TotalSeconds));

                    newRecords.Add(newRecord);
                }

                context.AddRange(newRecords);
                await context.SaveChangesAsync();

                LastPersistedAt = now;
            }
        }
    }
}
