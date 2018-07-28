using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold_Model;

namespace TW.Vault.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public static int PageSize => Configuration.Instance.GetValue("PageSize", 100);
        protected readonly VaultContext context;

        public ControllerBase(VaultContext context)
        {
            this.context = context;
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

        protected async Task<IActionResult> FindOr404<T>(params object[] ids) where T : class
        {
            T entity = await context.FindAsync<T>(ids);

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }
    }
}
