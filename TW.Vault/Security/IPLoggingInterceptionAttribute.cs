using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Security
{
    public class IPLoggingInterceptionAttribute : ActionFilterAttribute
    {
        private static ILogger _logger = null;
        private static ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = Log.ForContext<IPLoggingInterceptionAttribute>();
                return _logger;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerType = context.Controller.GetType();
            var requestMethod = context.HttpContext.Request.Method;
            var ip = context.HttpContext.Connection.RemoteIpAddress;
            var target = context.HttpContext.Request.Path;
            var headers = context.HttpContext.Request.Headers;

            logger.Information($"{requestMethod} request from {ip} to {controllerType.Name}::{target} with {headers.Count} headers ({String.Join(", ", headers.Keys)})");
            base.OnActionExecuting(context);
        }
    }
}
