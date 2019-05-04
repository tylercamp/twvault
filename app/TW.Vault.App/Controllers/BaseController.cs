using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using TW.Vault.Features;
using TW.Vault.Scaffold;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    public abstract class BaseController : Controller
    {
        public static int PageSize => Configuration.Instance.GetValue("PageSize", 100);
        protected readonly VaultContext context;
        protected readonly ILogger logger;
        private readonly IServiceScopeFactory scopeFactory;

        protected ConcurrentDictionary<String, TimeSpan> ProfilingEntries { get; } = new ConcurrentDictionary<string, TimeSpan>();

        public BaseController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.scopeFactory = scopeFactory;
        }


        //  Query helpers

        //  Returns a "page" of ie 100 elements (max) depending on `?page=` in the request URL
        protected IQueryable<T> Paginated<T>(IQueryable<T> set) where T : class
        {
            int page = 0;

            String pageParam = this.Request.Query["page"];
            if (pageParam != null)
            {
                int.TryParse(pageParam, out page);
            }

            if (page < 0)
                page = 0;

            return set.Skip(PageSize * page).Take(PageSize);
        }

        //  Searches the given table (DbSet) with the given query (enhancer) and converts it to some time via 'selector'
        protected async Task<IActionResult> SelectOr404<T>(Func<DbSet<T>, IQueryable<T>> enhancer, Func<T, object> selector) where T : class
        {
            var query = enhancer(context.Set<T>());

            T entity = await (
                    from entry in query
                    select entry
                ).FirstOrDefaultAsync();

            if (entity == null)
                return NotFound();
            else
                return Ok(selector(entity));
        }

        protected class CurrentContextDbSets
        {
            int worldId, accessGroupId;
            VaultContext context;
            public CurrentContextDbSets(VaultContext context, int worldId, int accessGroupId)
            {
                this.worldId = worldId;
                this.accessGroupId = accessGroupId;
                this.context = context;
            }

            public IQueryable<Ally> Ally => context.Ally.FromWorld(worldId);
            public IQueryable<Command> Command => context.Command.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<CommandArmy> CommandArmy => context.CommandArmy.FromWorld(worldId);
            public IQueryable<Conquer> Conquer => context.Conquer.FromWorld(worldId);
            public IQueryable<CurrentArmy> CurrentArmy => context.CurrentArmy.FromWorld(worldId);
            public IQueryable<CurrentBuilding> CurrentBuilding => context.CurrentBuilding.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<CurrentPlayer> CurrentPlayer => context.CurrentPlayer.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<CurrentVillage> CurrentVillage => context.CurrentVillage.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<CurrentVillageSupport> CurrentVillageSupport => context.CurrentVillageSupport.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<EnemyTribe> EnemyTribe => context.EnemyTribe.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<Player> Player => context.Player.FromWorld(worldId);
            public IQueryable<Report> Report => context.Report.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<IgnoredReport> IgnoredReport => context.IgnoredReport.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<ReportArmy> ReportArmy => context.ReportArmy.FromWorld(worldId);
            public IQueryable<ReportBuilding> ReportBuilding => context.ReportBuilding.FromWorld(worldId);
            public IQueryable<Transaction> Transaction => context.Transaction.FromWorld(worldId);
            public IQueryable<User> User => context.User.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<UserLog> UserLog => context.UserLog.FromWorld(worldId).FromAccessGroup(accessGroupId);
            public IQueryable<Village> Village => context.Village.FromWorld(worldId);



            public IQueryable<User> ActiveUser => User.Active();
        }

        CurrentContextDbSets _currentSets = null;
        protected CurrentContextDbSets CurrentSets
        {
            get
            {
                if (_currentSets == null)
                    _currentSets = new CurrentContextDbSets(context, CurrentWorldId, CurrentAccessGroupId);
                return _currentSets;
            }
        }



        //  Security helpers

        protected int CurrentUserId => (int)HttpContext.Items["UserId"];
        protected long CurrentPlayerId => (long)HttpContext.Items["PlayerId"];
        protected short CurrentUserPermissions => (short)HttpContext.Items["UserPermissions"];
        protected bool IsSitter => (bool)HttpContext.Items["UserIsSitter"];
        protected long CurrentTribeId => (long)HttpContext.Items["TribeId"];
        protected Guid CurrentAuthToken => (Guid)HttpContext.Items["AuthToken"];
        protected int CurrentAccessGroupId => (int)HttpContext.Items["AccessGroupId"];

        /// <summary>
        /// Warning - Try not to use this directly, as it won't include permissions based on whether the
        /// current user is being sat.
        /// </summary>
        protected User CurrentUser => (User)HttpContext.Items["User"];



        protected IPAddress UserIP => Request.HttpContext.Connection.RemoteIpAddress;

        protected bool CurrentUserIsAdmin => CurrentUserPermissions >= (short)PermissionLevel.Admin;
        protected bool CurrentUserIsSystem => CurrentUserPermissions >= (short)PermissionLevel.System;

        protected Transaction BuildTransaction(long? previousTransactionId = null) => new Transaction
        {
            Ip = HttpContext.Connection.RemoteIpAddress,
            OccurredAt = DateTime.UtcNow,
            Uid = CurrentUserId,
            WorldId = CurrentWorld.Id,
            PreviousTxId = previousTransactionId
        };

        protected InvalidDataRecord MakeInvalidDataRecord(String data, String reason) => new InvalidDataRecord
        {
            UserId = CurrentUserId,
            DataString = data,
            Reason = reason,
            Endpoint = $"{Request.Method}:{Request.Path.Value}"
        };

        protected FailedAuthorizationRecord MakeFailedAuthRecord(String reason) => new FailedAuthorizationRecord
        {
            Ip = UserIP,
            OccurredAt = DateTime.UtcNow,
            PlayerId = CurrentPlayerId,
            RequestedEndpoint = Request.Path.ToString(),
            TribeId = CurrentTribeId,
            WorldId = CurrentWorldId,
            Reason = reason
        };


        //  Helpers for TW-related stuff

        protected String CurrentWorldName => RouteData.Values["worldName"] as String;

        //  Don't want to query the database for World Name -> World ID every time, those values won't change
        private static ConcurrentDictionary<String, short> CachedWorldIds = new ConcurrentDictionary<string, short>();
        protected short CurrentWorldId => CachedWorldIds.GetOrAdd(CurrentWorldName, (k) => CurrentWorld.Id);

        private World _currentWorld = null;
        protected World CurrentWorld
        {
            get
            {
                if (_currentWorld == null)
                {
                    if (!CachedWorldIds.ContainsKey(CurrentWorldName))
                    {
                        _currentWorld = (
                                from world in context.World.Include(w => w.WorldSettings).Include(w => w.DefaultTranslation)
                                where world.Name == CurrentWorldName
                                select world
                            ).FirstOrDefault();
                    }
                    else
                    {
                        _currentWorld = (
                                from world in context.World.Include(w => w.WorldSettings)
                                where world.Id == CurrentWorldId
                                select world
                            ).FirstOrDefault();
                    }
                }

                return _currentWorld;
            }
        }

        protected WorldSettings CurrentWorldSettings => CurrentWorld.WorldSettings;

        protected DateTime CurrentServerTime => CurrentWorldSettings.ServerTime;

        protected TimeSpan TimeUntil(DateTime serverTime)
        {
            // Strip time specification
            var timeZone = DateTimeZoneProviders.Tzdb[CurrentWorldSettings.TimeZoneId];
            var currentServerTime = SystemClock.Instance.InZone(timeZone).GetCurrentZonedDateTime();
            var instant = Instant.FromDateTimeUtc(serverTime);
            var zonedTime = new ZonedDateTime(instant, timeZone);
            return (zonedTime - currentServerTime).ToTimeSpan();
        }

        //  In case world data needs to be pre-loaded
        protected bool PreloadWorldData()
        {
            _currentWorld = CurrentWorld;
            return _currentWorld != null;
        }

        protected void PreloadTranslationData()
        {
            _currentTranslation = CurrentTranslation;
        }



        //  Translations
        private short? _currentTranslationId;
        protected short CurrentTranslationId
        {
            get
            {
                if (_currentTranslationId == null)
                {
                    if (Request.Headers.ContainsKey("X-V-TRANSLATION-ID"))
                    {
                        short parsedTranslationId;
                        if (short.TryParse(Request.Headers["X-V-TRANSLATION-ID"], out parsedTranslationId))
                            _currentTranslationId = parsedTranslationId;
                    }

                    if (_currentTranslationId == null)
                        _currentTranslationId = CurrentWorld.DefaultTranslationId;

                }

                return _currentTranslationId.Value;
            }
        }

        private TranslationRegistry _currentTranslation;
        protected TranslationRegistry CurrentTranslation
        {
            get
            {
                TranslationRegistry LoadTranslation(short id) => context
                    .TranslationRegistry
                    .Include(r => r.Language)
                    .Include(r => r.Entries)
                        .ThenInclude(e => e.Key)
                    .Where(r => r.Id == id)
                    .FirstOrDefault();

                if (_currentTranslation == null)
                {
                    _currentTranslation = LoadTranslation(CurrentTranslationId);
                    if (_currentTranslation == null)
                    {
                        _currentTranslationId = CurrentWorld.DefaultTranslationId;
                        _currentTranslation = LoadTranslation(CurrentTranslationId);
                    }
                }

                return _currentTranslation;
            }
        }

        private TranslationContext _translation;
        protected TranslationContext Translation
        {
            get
            {
                if (_translation == null)
                    _translation = new TranslationContext(context, CurrentTranslation, CurrentWorld.DefaultTranslationId, Configuration.Translation.BaseTranslationId);

                return _translation;
            }
        }

        protected String Translate(String keyName, object parameters = null) => TranslateAsync(keyName, parameters).Result;

        protected async Task<String> TranslateAsync(String keyName, object parameters = null)
        {
            Dictionary<String, String> ParametersAsDictionary()
            {
                var dict = new Dictionary<String, String>();
                if (parameters == null)
                    return dict;

                foreach (var property in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (property.CanRead)
                        dict.Add(property.Name, property.GetValue(parameters).ToString());
                }

                return dict;
            }

            var key = await context
                        .TranslationKey
                        .Include(k => k.Parameters)
                        .Where(k => k.Name == keyName)
                        .FirstOrDefaultAsync();

            if (key == null)
                throw new KeyNotFoundException("No key exists named: " + keyName);

            var providedParameters = ParametersAsDictionary();
            if (providedParameters.Count > 0 && key.Parameters == null)
                throw new InvalidOperationException($"Parameters given for key {keyName} but this key does not take parameters");

            var entry = Translation[key.Id];
            var result = entry.Value;

            if (key.Parameters == null)
                return result;

            var missingParams = key.Parameters.Select(p => p.Name).Except(providedParameters.Keys).ToList();
            var extraParams = providedParameters.Keys.Except(key.Parameters.Select(p => p.Name)).ToList();

            if (missingParams.Any())
                throw new InvalidOperationException($"Cannot translate key {keyName} without the parameters: {String.Join(", ", missingParams)}");
            if (extraParams.Any())
                throw new InvalidOperationException($"Unnecessary parameters provided while translating key {keyName}: {String.Join(", ", extraParams)}");

            foreach (var (paramName, paramValue) in providedParameters.Tupled())
                result = result.Replace($"{{{paramName}}}", paramValue);

            return result;
        }

        protected async Task<T> WithTemporaryContext<T>(Func<VaultContext, Task<T>> op)
        {
            using (var scope = scopeFactory.CreateScope())
            using (var tempContext = scope.ServiceProvider.GetRequiredService<VaultContext>())
                return await op(tempContext);
        }

        protected Task<T> WithTemporarySets<T>(Func<CurrentContextDbSets, Task<T>> op)
        {
            return WithTemporaryContext((ctx) =>
            {
                var tempSets = new CurrentContextDbSets(ctx, CurrentWorldId, CurrentAccessGroupId);
                return op(tempSets);
            });
        }



        //  Performance profiling helpers

        private String FormatProfileLabel(String label)
        {
            return $"[{Request.Method}] {GetType().Name}:{RouteData.Values["action"]} - {label}";
        }

        protected void Profile(String label, Action action)
        {
            label = FormatProfileLabel(label);

            DateTime start = DateTime.UtcNow;
            action();
            var duration = DateTime.UtcNow - start;
            Profiling.AddRecord(label, duration);
            ProfilingEntries.TryAdd(label, duration);

            logger.LogDebug("{0} took {1}ms", label, (int)duration.TotalMilliseconds);
        }

        protected T Profile<T>(String label, Func<T> func)
        {
            label = FormatProfileLabel(label);

            DateTime start = DateTime.UtcNow;
            T result = func();
            var duration = DateTime.UtcNow - start;
            Profiling.AddRecord(label, duration);
            ProfilingEntries.TryAdd(label, duration);

            logger.LogDebug("{0} took {1}ms", label, (int)duration.TotalMilliseconds);
            return result;
        }

        protected async Task Profile(String label, Func<Task> func)
        {
            label = FormatProfileLabel(label);

            DateTime start = DateTime.UtcNow;
            await func();
            var duration = DateTime.UtcNow - start;
            Profiling.AddRecord(label, duration);
            ProfilingEntries.TryAdd(label, duration);

            logger.LogDebug("{0} took {1}ms", label, (int)duration.TotalMilliseconds);
        }

        protected async Task<T> Profile<T>(String label, Func<Task<T>> func)
        {
            label = FormatProfileLabel(label);

            DateTime start = DateTime.UtcNow;
            T result = await func();
            var duration = DateTime.UtcNow - start;
            Profiling.AddRecord(label, duration);
            ProfilingEntries.TryAdd(label, duration);

            logger.LogDebug("{0} took {1}ms", label, (int)duration.TotalMilliseconds);
            return result;
        }
    }
}
