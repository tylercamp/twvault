using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold_Model;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public static int PageSize => Configuration.Instance.GetValue("PageSize", 100);
        protected readonly VaultContext context;
        protected readonly ILogger logger;

        public ControllerBase(VaultContext context, ILoggerFactory loggerFactory)
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

        protected async Task<IActionResult> FindOr404<T>(object id) where T : class
        {
            T entity = await context.FindAsync<T>(id);

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }

        protected async Task<IActionResult> FindOr404<T>(object id, Func<T, object> selector) where T : class
        {
            T entity = await context.FindAsync<T>(id);

            if (entity == null)
                return NotFound();
            else
                return Ok(selector(entity));
        }

        protected async Task<IActionResult> FindOr404<T>(Func<DbSet<T>, IQueryable<T>> enhancer, Func<T, object> selector) where T : class
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

        protected T GetOrAddEntity<T>(T entity) where T : class, new()
        {
            if (entity != null)
                return entity;
            else
                return context.Add(new T()).Entity;
        }

        protected User CurrentUser => HttpContext.Items["User"] as User;

        protected bool CurrentUserIsAdmin => CurrentUser.PermissionsLevel >= (short)PermissionLevel.Admin;
        protected bool CurrentUserIsSystem => CurrentUser.PermissionsLevel >= (short)PermissionLevel.System;

        protected Transaction BuildTransaction() => new Transaction
        {
            Ip = HttpContext.Connection.RemoteIpAddress,
            OccurredAt = DateTime.UtcNow,
            Uid = CurrentUser.Uid
        };
    }
}
