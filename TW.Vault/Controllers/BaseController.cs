using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        protected User CurrentUser => HttpContext.Items["User"] as User;

        protected bool CurrentUserIsAdmin => CurrentUser.PermissionsLevel >= (short)PermissionLevel.Admin;
        protected bool CurrentUserIsSystem => CurrentUser.PermissionsLevel >= (short)PermissionLevel.System;

        protected Transaction BuildTransaction() => new Transaction
        {
            Ip = HttpContext.Connection.RemoteIpAddress,
            OccurredAt = DateTime.UtcNow,
            Uid = CurrentUser.Uid,
            WorldId = CurrentWorld.Id
        };

        protected Scaffold.InvalidDataRecord MakeInvalidDataRecord(String data, String reason) => new InvalidDataRecord
        {
            UserId = CurrentUser.Uid,
            DataString = data,
            Reason = reason,
            Endpoint = $"{Request.Method}:{Request.Path.Value}"
        };

        protected String CurrentWorldName => RouteData.Values["worldName"] as String;

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
                                from world in context.World
                                where world.Name == CurrentWorldName
                                select world
                            ).FirstOrDefault();
                    }
                    else
                    {
                        _currentWorld = context.World.Find(CurrentWorldId);
                    }
                }

                return _currentWorld;
            }
        }

        protected long CurrentTribeId => (long)HttpContext.Items["TribeId"];

        //  TODO - This should change per requested server
        protected DateTime CurrentServerTime => DateTime.Now + TimeSpan.FromHours(1);

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
