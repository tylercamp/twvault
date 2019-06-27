using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TW.Vault.Features;

namespace TW.Vault.MapDataFetcher
{
    public class DataFetchingService : BackgroundService
    {
        public DataFetchingService(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime) : base(scopeFactory, loggerFactory)
        {
            Instance = this;
            this.applicationLifetime = applicationLifetime;
        }

        public static DataFetchingService Instance { get; private set; }
        private bool forceRefresh = false;
        private bool forceReApply = false;
        private FetchJobStats pendingStats = null;
        private DateTime lastCheckedAt = DateTime.MinValue;
        private IApplicationLifetime applicationLifetime;

        public FetchJobStats LatestStats { get; private set; } = null;

        public async Task<FetchJobStats> ForceRefresh(bool forceReApply)
        {
            this.forceRefresh = true;
            this.forceReApply = forceReApply;
            while (forceRefresh)
                await Task.Delay(100);
            return LatestStats;
        }

        private static String FileCachingPath => Configuration.Instance["CachingFilePath"] ?? "cache";
        private static int DataBatchSize => int.Parse(Configuration.Instance["DataBatchSize"]);
        private static int CheckDelaySeconds => int.Parse(Configuration.Instance["CheckDelaySeconds"] ?? "60");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InternalExecuteAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception e)
            {
                logger.LogError(e, $"An exception occurred in {nameof(DataFetchingService)}, terminating");
            }
            finally
            {
                applicationLifetime.StopApplication();
            }
        }

        private async Task InternalExecuteAsync(CancellationToken stoppingToken)
        {
            var cachingPath = FileCachingPath;
            if (!Directory.Exists(cachingPath))
                Directory.CreateDirectory(cachingPath);

            logger.LogInformation("Starting up...");
            logger.LogInformation("Using connection string: {connectionString}", Configuration.ConnectionString);

            while (!stoppingToken.IsCancellationRequested)
            {
                await WithVaultContext(async ctx =>
                {
                    logger.LogDebug("Checking for fetch jobs");
                    var jobs = GetFetchingJobs(ctx, DateTime.UtcNow, forceReApply);
                    if (stoppingToken.IsCancellationRequested || jobs.Count == 0)
                        return;

                    logger.LogInformation("Found {numJobs} jobs for: [{worlds}]", jobs.Count, String.Join(", ", jobs.Select(j => j.WorldName)));

                    var sw = Stopwatch.StartNew();

                    using (var client = new HttpClient())
                    {
                        if (jobs.Count > 0)
                            pendingStats = new FetchJobStats();

                        foreach (var job in jobs)
                        {
                            try
                            {
                                logger.LogInformation("Running job for {world}/id={id}", job.WorldName, job.WorldId);
                                String villageData, playerData, conquerData, tribeData;

                                var currentWorldStats = new FetchWorldJobStats();
                                pendingStats.StatsByWorld.Add(job.WorldName, currentWorldStats);

                                if (job.NeedsRefresh)
                                {
                                    logger.LogInformation("Fetching data from web...");

                                    var villageRequest = await client.GetAsync(job.VillageDataUrl, stoppingToken);
                                    var playerRequest = await client.GetAsync(job.PlayerDataUrl, stoppingToken);
                                    var conquerRequest = await client.GetAsync(job.ConquerDataUrl, stoppingToken);
                                    var tribeRequest = await client.GetAsync(job.TribeDataUrl, stoppingToken);

                                    if (stoppingToken.IsCancellationRequested)
                                        break;

                                    if (!villageRequest.IsSuccessStatusCode)
                                        logger.LogError("GET request to {url} responded with {message} ({code})", job.VillageDataUrl, villageRequest.ReasonPhrase, villageRequest.StatusCode);
                                    if (!playerRequest.IsSuccessStatusCode)
                                        logger.LogError("GET request to {url} responded with {message} ({code})", job.PlayerDataUrl, playerRequest.ReasonPhrase, playerRequest.StatusCode);
                                    if (!conquerRequest.IsSuccessStatusCode)
                                        logger.LogError("GET request to {url} responded with {message} ({code})", job.ConquerDataUrl, conquerRequest.ReasonPhrase, conquerRequest.StatusCode);
                                    if (!tribeRequest.IsSuccessStatusCode)
                                        logger.LogError("GET request to {url} responded with {message} ({code})", job.TribeDataUrl, tribeRequest.ReasonPhrase, tribeRequest.StatusCode);

                                    villageData = await villageRequest.Content.ReadAsStringAsync();
                                    playerData = await playerRequest.Content.ReadAsStringAsync();
                                    conquerData = await conquerRequest.Content.ReadAsStringAsync();
                                    tribeData = await tribeRequest.Content.ReadAsStringAsync();

                                    if (stoppingToken.IsCancellationRequested)
                                        break;

                                    await File.WriteAllTextAsync(job.VillageDataFilePath, villageData, stoppingToken);
                                    await File.WriteAllTextAsync(job.PlayerDataFilePath, playerData, stoppingToken);
                                    await File.WriteAllTextAsync(job.ConquerDataFilePath, conquerData, stoppingToken);
                                    await File.WriteAllTextAsync(job.TribeDataFilePath, tribeData, stoppingToken);

                                    if (stoppingToken.IsCancellationRequested)
                                        break;
                                }
                                else
                                {
                                    logger.LogInformation("Loading from disk...");
                                    villageData = await File.ReadAllTextAsync(job.VillageDataFilePath, stoppingToken);
                                    playerData = await File.ReadAllTextAsync(job.PlayerDataFilePath, stoppingToken);
                                    conquerData = await File.ReadAllTextAsync(job.ConquerDataFilePath, stoppingToken);
                                    tribeData = await File.ReadAllTextAsync(job.TribeDataFilePath, stoppingToken);

                                    if (stoppingToken.IsCancellationRequested)
                                        break;
                                }

                                logger.LogInformation("Loaded data");

                                var villageDataParts = villageData.Split('\n').Where(s => s.Length > 0);
                                var playerDataParts = playerData.Split('\n').Where(s => s.Length > 0);
                                var conquerDataParts = conquerData.Split('\n').Where(s => s.Length > 0);
                                var tribeDataParts = tribeData.Split('\n').Where(s => s.Length > 0);

                                logger.LogInformation("Uploading data to DB...");
                                await UploadTribeData(job.WorldId, tribeDataParts, currentWorldStats, stoppingToken); if (stoppingToken.IsCancellationRequested) break;
                                await UploadPlayerData(job.WorldId, playerDataParts, currentWorldStats, stoppingToken); if (stoppingToken.IsCancellationRequested) break;
                                await UploadVillageData(job.WorldId, villageDataParts, currentWorldStats, stoppingToken); if (stoppingToken.IsCancellationRequested) break;
                                await UploadConquerData(job.WorldId, conquerDataParts, currentWorldStats, stoppingToken); if (stoppingToken.IsCancellationRequested) break;

                                if (stoppingToken.IsCancellationRequested)
                                    break;
                            }
                            catch (TaskCanceledException e) { }
                            catch (Exception e)
                            {
                                logger.LogWarning("An exception occurred while processing for {world}/id={id}: {exception}", job.WorldName, job.WorldId, e);
                            }
                        }
                    }

                    if (jobs.Count > 0)
                        logger.LogInformation("Finished in {ms}ms", sw.ElapsedMilliseconds);

                    if (pendingStats != null)
                        pendingStats.Duration = sw.Elapsed;
                });

                if (pendingStats != null)
                {
                    pendingStats.TotalConquersUpdated = pendingStats.StatsByWorld.Values.Sum(s => s.NumConquersCreated);
                    pendingStats.TotalVillagesUpdated = pendingStats.StatsByWorld.Values.Sum(s => s.NumVillagesUpdated + s.NumVillagesCreated);
                    pendingStats.TotalTribesUpdated = pendingStats.StatsByWorld.Values.Sum(s => s.NumTribesCreated + s.NumTribesUpdated);
                    pendingStats.TotalPlayersUpdated = pendingStats.StatsByWorld.Values.Sum(s => s.NumPlayersCreated + s.NumPlayersUpdated);
                    LatestStats = pendingStats;
                    pendingStats = null;
                }

                forceReApply = false;
                forceRefresh = false;
                lastCheckedAt = DateTime.UtcNow;
                while (!stoppingToken.IsCancellationRequested && !forceRefresh && (DateTime.UtcNow - lastCheckedAt) < TimeSpan.FromSeconds(CheckDelaySeconds))
                    await Task.Delay(100, stoppingToken);
            }
        }

        private async Task UploadVillageData(short worldId, IEnumerable<String> villageDataParts, FetchWorldJobStats currentStats, CancellationToken stoppingToken)
        {
            logger.LogDebug("Uploading village data...");

            var batchedVillageData = villageDataParts
                .Select(s => s.Split(','))
                .Select(p => new
                {
                    VillageId = long.Parse(p[0]),
                    VillageName = p[1],
                    X = short.Parse(p[2]),
                    Y = short.Parse(p[3]),
                    PlayerId = long.Parse(p[4]) == 0 ? null : (long?)long.Parse(p[4]),
                    Points = short.Parse(p[5]),
                    Rank = int.Parse(p[6])
                })
                .Grouped(DataBatchSize)
                .Select(g => g.ToList())
                .ToList();

            logger.LogDebug("Made {cnt} batches", batchedVillageData.Count);

            foreach (var batch in batchedVillageData)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                await WithVaultContext(async (context) =>
                {
                    var villageIds = batch.Select(v => v.VillageId).ToList();

                    var existingVillages = await context.Village.FromWorld(worldId).Where(v => villageIds.Contains(v.VillageId)).ToListAsync(stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    var existingVillageIds = existingVillages.Select(v => v.VillageId).ToList();
                    var scaffoldVillages = existingVillages
                        .Concat(villageIds
                            .Except(existingVillageIds)
                            .Select(id => new Scaffold.Village { VillageId = id })
                        )
                        .ToDictionary(v => v.VillageId, v => v);

                    foreach (var entry in batch)
                    {
                        var village = scaffoldVillages[entry.VillageId];
                        if (village.WorldId != worldId) village.WorldId = worldId;
                        if (village.VillageName != entry.VillageName) village.VillageName = entry.VillageName;
                        if (village.X != entry.X) village.X = entry.X;
                        if (village.Y != entry.Y) village.Y = entry.Y;
                        if (village.PlayerId != entry.PlayerId) village.PlayerId = entry.PlayerId;
                        if (village.VillageRank != entry.Rank) village.VillageRank = entry.Rank;
                        if (village.Points != entry.Points) village.Points = entry.Points;

                        if (!existingVillageIds.Contains(entry.VillageId))
                            context.Add(village);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        return;

                    context.ChangeTracker.DetectChanges();
                    currentStats.NumVillagesCreated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Count();
                    currentStats.NumVillagesUpdated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Count();

                    await context.SaveChangesAsync(stoppingToken);
                });

                logger.LogDebug("Uploaded batch of {cnt} villages", batch.Count);
            }

            logger.LogDebug("Finished uploading village data");
        }

        private async Task UploadPlayerData(short worldId, IEnumerable<String> playerDataParts, FetchWorldJobStats currentStats, CancellationToken stoppingToken)
        {
            logger.LogDebug("Uploading player data...");

            var batchedPlayerData = playerDataParts
                .Select(s => s.Split(','))
                .Select(p => new
                {
                    PlayerId = long.Parse(p[0]),
                    PlayerName = p[1],
                    TribeId = long.Parse(p[2]) == 0 ? null : (long?)long.Parse(p[2]),
                    Villages = int.Parse(p[3]),
                    Points = int.Parse(p[4]),
                    Rank = int.Parse(p[5])
                })
                .Grouped(DataBatchSize)
                .Select(g => g.ToList())
                .ToList();

            if (batchedPlayerData.SelectMany(b => b).Count() != playerDataParts.Count())
                throw new Exception();

            foreach (var batch in batchedPlayerData)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await WithVaultContext(async context =>
                {
                    var playerIds = batch.Select(p => p.PlayerId).ToList();

                    var existingPlayers = await context.Player.FromWorld(worldId).Where(p => playerIds.Contains(p.PlayerId)).ToListAsync(stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    var existingPlayerIds = existingPlayers.Select(p => p.PlayerId).ToList();
                    var scaffoldPlayers = existingPlayers
                        .Concat(playerIds
                            .Except(existingPlayerIds)
                            .Select(id => new Scaffold.Player { PlayerId = id })
                        )
                        .ToDictionary(p => p.PlayerId, p => p);

                    foreach (var entry in batch)
                    {
                        var player = scaffoldPlayers[entry.PlayerId];
                        if (player.WorldId != worldId) player.WorldId = worldId;
                        if (player.PlayerName != entry.PlayerName) player.PlayerName = entry.PlayerName;
                        if (player.TribeId != entry.TribeId) player.TribeId = entry.TribeId;
                        if (player.Villages != entry.Villages) player.Villages = entry.Villages;
                        if (player.Points != entry.Points) player.Points = entry.Points;
                        if (player.PlayerRank != entry.Rank) player.PlayerRank = entry.Rank;

                        if (!existingPlayerIds.Contains(entry.PlayerId))
                            context.Add(player);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        return;

                    context.ChangeTracker.DetectChanges();
                    currentStats.NumPlayersCreated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Count();
                    currentStats.NumPlayersUpdated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Count();

                    await context.SaveChangesAsync(stoppingToken);
                });

                logger.LogDebug("Uploaded batch of {cnt} players", batch.Count);
            }

            logger.LogDebug("Finished uploading player data");
        }

        private async Task UploadConquerData(short worldId, IEnumerable<String> conquerDataParts, FetchWorldJobStats currentStats, CancellationToken stoppingToken)
        {
            logger.LogDebug("Uploading conquer data...");

            // Can't do "WHERE (X._, X._, ...) IN ((...), (...))" via ef-core, so need to do this in-memory

            var conquerData = conquerDataParts
                .Select(s => s.Split(','))
                .Select(p => new
                {
                    VillageId = long.Parse(p[0]),
                    UnixTimestamp = long.Parse(p[1]),
                    NewOwner = long.Parse(p[2]) == 0 ? null : (long?)long.Parse(p[2]),
                    OldOwner = long.Parse(p[3]) == 0 ? null : (long?)long.Parse(p[3])
                })
                .ToList();

            logger.LogDebug("Fetching existing conquers...");
            var existingConquers = await WithVaultContext(context => context.Conquer.FromWorld(worldId).AsNoTracking().ToListAsync(stoppingToken));
            if (stoppingToken.IsCancellationRequested)
                return;

            String MakeConquerSignature(long? villageId, long? unixTimestamp, long? newOwner, long? oldOwner) =>
                $"{villageId.Value}-{unixTimestamp.Value}-{newOwner ?? 0}-{oldOwner ?? 0}";

            logger.LogDebug("Building signature set...");
            var conquerSignatures = new HashSet<String>();
            foreach (var conquer in existingConquers)
                conquerSignatures.Add(MakeConquerSignature(conquer.VillageId, conquer.UnixTimestamp, conquer.NewOwner, conquer.OldOwner));

            logger.LogDebug("Checking for new conquers...");
            int numAdded = 0;
            await WithVaultContext(async context =>
            {
                foreach (var entry in conquerData.Where(e => !conquerSignatures.Contains(MakeConquerSignature(e.VillageId, e.UnixTimestamp, e.NewOwner, e.OldOwner))))
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    context.Add(new Scaffold.Conquer
                    {
                        VillageId = entry.VillageId,
                        UnixTimestamp = entry.UnixTimestamp,
                        NewOwner = entry.NewOwner,
                        OldOwner = entry.OldOwner,
                        WorldId = worldId
                    });

                    if (stoppingToken.IsCancellationRequested)
                        return;

                    ++numAdded;
                }

                await context.SaveChangesAsync(stoppingToken);
            });

            currentStats.NumConquersCreated = numAdded;

            logger.LogDebug("Finished updating with {cnt} conquer items ({numAdded} added)", conquerData.Count, numAdded);
        }

        private async Task UploadTribeData(short worldId, IEnumerable<String> tribeDataParts, FetchWorldJobStats currentStats, CancellationToken stoppingToken)
        {
            logger.LogDebug("Uploading tribe data...");

            var batchedTribeData = tribeDataParts
                .Select(s => s.Split(','))
                .Select(p => new
                {
                    TribeId = long.Parse(p[0]),
                    TribeName = p[1],
                    Tag = p[2],
                    Members = int.Parse(p[3]),
                    Villages = int.Parse(p[4]),
                    Points = long.Parse(p[5]),
                    AllPoints = long.Parse(p[6]),
                    TribeRank = long.Parse(p[7])
                })
                .Grouped(DataBatchSize)
                .Select(g => g.ToList())
                .ToList();

            foreach (var batch in batchedTribeData)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await WithVaultContext(async context =>
                {
                    var tribeIds = batch.Select(e => e.TribeId).ToList();

                    var existingTribes = await context.Ally.FromWorld(worldId).Where(t => tribeIds.Contains(t.TribeId)).ToListAsync(stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    var existingTribeIds = existingTribes.Select(t => t.TribeId).ToList();
                    var scaffoldTribes = existingTribes
                        .Concat(tribeIds
                            .Except(existingTribeIds)
                            .Select(tid => new Scaffold.Ally { TribeId = tid })
                        )
                        .ToDictionary(t => t.TribeId, t => t);

                    foreach (var entry in batch)
                    {
                        var tribe = scaffoldTribes[entry.TribeId];
                        if (tribe.WorldId != worldId) tribe.WorldId = worldId;
                        if (tribe.TribeName != entry.TribeName) tribe.TribeName = entry.TribeName;
                        if (tribe.Tag != entry.Tag) tribe.Tag = entry.Tag;
                        if (tribe.Members != entry.Members) tribe.Members = entry.Members;
                        if (tribe.Villages != entry.Villages) tribe.Villages = entry.Villages;
                        if (tribe.Points != entry.Points) tribe.Points = entry.Points;
                        if (tribe.AllPoints != entry.AllPoints) tribe.AllPoints = entry.AllPoints;
                        if (tribe.TribeRank != entry.TribeRank) tribe.TribeRank = entry.TribeRank;

                        if (!existingTribeIds.Contains(entry.TribeId))
                            context.Add(tribe);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        return;

                    context.ChangeTracker.DetectChanges();
                    currentStats.NumTribesCreated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Count();
                    currentStats.NumTribesUpdated += context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Count();

                    await context.SaveChangesAsync(stoppingToken);
                });

                logger.LogDebug("Uploaded batch of {cnt} tribes", batch.Count);
            }

            logger.LogDebug("Finished uploading tribe data");
        }

        class FetchingJob
        {
            public FetchingJob(Scaffold.World world, String cachingPath)
            {
                WorldName = world.Name;
                WorldUrl = world.Hostname;
                WorldId = world.Id;

                this.cachingPath = cachingPath;
            }

            private String cachingPath;

            public String WorldName;
            public short WorldId;
            public String WorldUrl;
            public DateTime LastUpdatedAt =>
                DataFilePaths
                    .Where(f => File.Exists(f))
                    .Select(f => File.GetLastWriteTimeUtc(f))
                    .DefaultIfEmpty(DateTime.MinValue)
                    .Min();

            public bool NeedsRefresh => DateTime.UtcNow - LastUpdatedAt >= TimeSpan.FromHours(1);

            private IEnumerable<String> DataFilePaths => new[] { VillageDataFilePath, PlayerDataFilePath, ConquerDataFilePath, TribeDataFilePath };

            public String VillageDataFilePath => Path.Combine(this.cachingPath, $"{WorldName}_village.csv");
            public String PlayerDataFilePath => Path.Combine(this.cachingPath, $"{WorldName}_player.csv");
            public String ConquerDataFilePath => Path.Combine(this.cachingPath, $"{WorldName}_conquer.csv");
            public String TribeDataFilePath => Path.Combine(this.cachingPath, $"{WorldName}_tribe.csv");

            public String VillageDataUrl => $"https://{WorldUrl}/map/village.txt";
            public String PlayerDataUrl => $"https://{WorldUrl}/map/player.txt";
            public String ConquerDataUrl => $"https://{WorldUrl}/map/conquer.txt";
            public String TribeDataUrl => $"https://{WorldUrl}/map/ally.txt";
        }

        private List<FetchingJob> GetFetchingJobs(Scaffold.VaultContext context, DateTime now, bool forceReApply)
        {
            var maxAge = TimeSpan.FromHours(1);
            var cachingPath = FileCachingPath;
            var worlds = context.World.ToList();
            return worlds.Select(w => new FetchingJob(w, cachingPath)).Where(j => forceReApply || j.NeedsRefresh).ToList();
        }
    }
}
