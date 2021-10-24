using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib.Security
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequirePermissionsAttribute : Attribute, IActionFilter
    {
        PermissionLevel minimumPermissions;

        public RequirePermissionsAttribute(PermissionLevel minimumPermissions)
        {
            this.minimumPermissions = minimumPermissions;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.Items["User"] as Scaffold.User;
            if (user.PermissionsLevel < (short) minimumPermissions)
            {

            }
        }
    }
}
