using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace TW.Vault
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExceptionInterceptionAttribute : ExceptionFilterAttribute
    {
        private static ILogger _logger = null;
        private static ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = Log.ForContext<ExceptionInterceptionAttribute>();
                return _logger;
            }
        }


        public override void OnException(ExceptionContext context)
        {
            if (!(context.Exception is TaskCanceledException))
                logger.Error("Exception thrown at endpoint: {endpoint}", context.HttpContext.Request.Path.Value);
            base.OnException(context);
        }
    }
}
