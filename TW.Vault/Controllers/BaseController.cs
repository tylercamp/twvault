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



        //  Security helpers

        //  "User" assigned in RequireAuthAttribute, when the request is first made
        protected User CurrentUser => HttpContext.Items["User"] as User;

        protected IPAddress UserIP => Request.HttpContext.Connection.RemoteIpAddress;

        protected bool CurrentUserIsAdmin => CurrentUser.PermissionsLevel >= (short)PermissionLevel.Admin;
        protected bool CurrentUserIsSystem => CurrentUser.PermissionsLevel >= (short)PermissionLevel.System;

        protected Transaction BuildTransaction(Transaction previousTransaction = null) => new Transaction
        {
            Ip = HttpContext.Connection.RemoteIpAddress,
            OccurredAt = DateTime.UtcNow,
            Uid = CurrentUser.Uid,
            WorldId = CurrentWorld.Id,
            PreviousTxId = previousTransaction?.TxId
        };

        protected InvalidDataRecord MakeInvalidDataRecord(String data, String reason) => new InvalidDataRecord
        {
            UserId = CurrentUser.Uid,
            DataString = data,
            Reason = reason,
            Endpoint = $"{Request.Method}:{Request.Path.Value}"
        };

        protected FailedAuthorizationRecord MakeFailedAuthRecord(String reason) => new FailedAuthorizationRecord
        {
            Ip = UserIP,
            OccurredAt = DateTime.UtcNow,
            PlayerId = CurrentUser.PlayerId,
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

        protected long CurrentTribeId => (long)HttpContext.Items["TribeId"];
        
        protected DateTime CurrentServerTime => DateTime.UtcNow + CurrentWorldSettings.UtcOffset;



        //  Performance profiling helpers

        private String FormatProfileLabel(String label)
        {
            return $"[{Request.Method}] {GetType().Name}:{RouteData.Values["action"]} - {label}";
        }

        protected void Profile(String label, Action action)
        {
            DateTime start = DateTime.UtcNow;
            action();
            Profiling.AddRecord(FormatProfileLabel(label), DateTime.UtcNow - start);
        }

        protected T Profile<T>(String label, Func<T> func)
        {
            DateTime start = DateTime.UtcNow;
            T result = func();
            Profiling.AddRecord(FormatProfileLabel(label), DateTime.UtcNow - start);
            return result;
        }

        protected async Task Profile(String label, Func<Task> func)
        {
            DateTime start = DateTime.UtcNow;
            await func();
            Profiling.AddRecord(FormatProfileLabel(label), DateTime.UtcNow - start);
        }

        protected async Task<T> Profile<T>(String label, Func<Task<T>> func)
        {
            DateTime start = DateTime.UtcNow;
            T result = await func();
            Profiling.AddRecord(FormatProfileLabel(label), DateTime.UtcNow - start);
            return result;
        }
    }
}
