using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scaffold = TW.Vault.Scaffold_Model;

namespace TW.Vault.Security
{
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        Scaffold.VaultContext dbContext;
        ILogger logger;

        public RequireAuthAttribute(Scaffold.VaultContext dbContext, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            this.logger = loggerFactory.CreateLogger<RequireAuthAttribute>();
        }

        private void LogFailedRequest(Scaffold.FailedAuthorizationRecord record)
        {
            logger.LogInformation("Failed request to {Path} from {IP} because: {Reason}", record.RequestedEndpoint, record.Ip, record.Reason);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authHeaders = AuthenticationUtil.ParseHeaders(context.HttpContext.Request.Headers);
            if (!authHeaders.IsValid)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                failedRequest.Reason = "Invalid authentication headers";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();

                LogFailedRequest(failedRequest);

                context.Result = new StatusCodeResult(401);
                return;
            }

            var authToken = authHeaders.AuthToken;
            var discoveredUser = (
                    from user in dbContext.User
                    where user.AuthToken == authToken
                    select user
                ).FirstOrDefault();

            var allUsers = dbContext.User.ToList();

            if (discoveredUser == null || !discoveredUser.Enabled)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                if (discoveredUser == null)
                    failedRequest.Reason = "No user found with given token";
                else
                    failedRequest.Reason = "Requested by disabled user";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();

                LogFailedRequest(failedRequest);

                context.Result = new StatusCodeResult(401);
                return;
            }

            if (discoveredUser.PlayerId != authHeaders.PlayerId.Value)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                failedRequest.Reason = "Player ID did not match the ID associated with the token";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();
            }

            context.HttpContext.Items["User"] = discoveredUser;
        }

        //public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        //{
        //    var authHeaders = AuthenticationUtil.ParseHeaders(context.HttpContext.Request.Headers);
        //    if (!authHeaders.IsValid)
        //    {
        //        var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
        //        failedRequest.Reason = "Invalid authentication headers";
        //        await dbContext.AddAsync(failedRequest);
        //        await dbContext.SaveChangesAsync();

        //        context.Result = new StatusCodeResult(401);
        //        return;
        //    }

        //    var authToken = authHeaders.AuthToken;
        //    var discoveredUser = await (
        //            from user in dbContext.User
        //            where user.AuthToken == authToken
        //            select user
        //        ).FirstOrDefaultAsync();

        //    if (discoveredUser == null || !discoveredUser.Enabled)
        //    {
        //        var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
        //        if (discoveredUser == null)
        //            failedRequest.Reason = "No user found with given token";
        //        else
        //            failedRequest.Reason = "Requested by disabled user";

        //        await dbContext.AddAsync(failedRequest);
        //        await dbContext.SaveChangesAsync();

        //        context.Result = new StatusCodeResult(401);
        //        return;
        //    }

        //    context.HttpContext.Items["User"] = discoveredUser;

        //    await next();
        //}
    }
}
