using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        protected ConcurrentDictionary<String, TimeSpan> ProfilingEntries { get; } = new ConcurrentDictionary<string, TimeSpan>();

        public BaseController(VaultContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);

            
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
            int worldId;
            VaultContext context;
            public CurrentContextDbSets(VaultContext context, int worldId)
            {
                this.worldId = worldId;
                this.context = context;
            }

            public IQueryable<Ally> Ally => context.Ally.FromWorld(worldId);
            public IQueryable<Command> Command => context.Command.FromWorld(worldId);
            public IQueryable<CommandArmy> CommandArmy => context.CommandArmy.FromWorld(worldId);
            public IQueryable<Conquer> Conquer => context.Conquer.FromWorld(worldId);
            public IQueryable<CurrentArmy> CurrentArmy => context.CurrentArmy.FromWorld(worldId);
            public IQueryable<CurrentBuilding> CurrentBuilding => context.CurrentBuilding.FromWorld(worldId);
            public IQueryable<CurrentPlayer> CurrentPlayer => context.CurrentPlayer.FromWorld(worldId);
            public IQueryable<CurrentVillage> CurrentVillage => context.CurrentVillage.FromWorld(worldId);
            public IQueryable<CurrentVillageSupport> CurrentVillageSupport => context.CurrentVillageSupport.FromWorld(worldId);
            public IQueryable<EnemyTribe> EnemyTribe => context.EnemyTribe.FromWorld(worldId);
            public IQueryable<Player> Player => context.Player.FromWorld(worldId);
            public IQueryable<Report> Report => context.Report.FromWorld(worldId);
            public IQueryable<ReportArmy> ReportArmy => context.ReportArmy.FromWorld(worldId);
            public IQueryable<ReportBuilding> ReportBuilding => context.ReportBuilding.FromWorld(worldId);
            public IQueryable<Transaction> Transaction => context.Transaction.FromWorld(worldId);
            public IQueryable<User> User => context.User.FromWorld(worldId);
            public IQueryable<UserLog> UserLog => context.UserLog.FromWorld(worldId);
            public IQueryable<Village> Village => context.Village.FromWorld(worldId);



            public IQueryable<User> ActiveUser => User.Where(u => u.Enabled && !u.IsReadOnly);
        }

        CurrentContextDbSets _currentSets = null;
        protected CurrentContextDbSets CurrentSets
        {
            get
            {
                if (_currentSets == null)
                    _currentSets = new CurrentContextDbSets(context, CurrentWorldId);
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
                                from world in context.World.Include(w => w.WorldSettings)
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
        
        protected DateTime CurrentServerTime => DateTime.UtcNow + CurrentWorldSettings.UtcOffset;

        //  In case world data needs to be pre-loaded
        protected void LoadWorldData()
        {
            _currentWorld = CurrentWorld;
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
