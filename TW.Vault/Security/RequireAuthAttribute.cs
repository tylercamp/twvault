using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features;

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
                failedRequest.Reason = "Invalid authentication headers: ";
                var exactReasons = new List<String>();
                if (authHeaders.AuthToken == null)
                    exactReasons.Add("AuthToken missing or invalid");
                if (authHeaders.PlayerId == null)
                    exactReasons.Add("PlayerId missing or invalid");
                if (authHeaders.TribeId == null)
                    exactReasons.Add("TribeId missing or invalid");

                failedRequest.Reason += String.Join(", ", exactReasons);

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();

                LogFailedRequest(failedRequest);

                context.Result = new StatusCodeResult(401);
                return;
            }

            var requestedWorld = context.RouteData.Values["worldName"] as String;
            if (requestedWorld == null)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                failedRequest.Reason = "Attempted to request a protected endpoint without worldName [programmer error]";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();

                LogFailedRequest(failedRequest);

                context.Result = new StatusCodeResult(401);
                return;
            }

            var world = dbContext.World.Where(w => w.Name == requestedWorld).FirstOrDefault();
            if (world == null)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                failedRequest.Reason = "Attempted to request a protected endpoint for a world that does not exist";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();

                LogFailedRequest(failedRequest);

                context.Result = new StatusCodeResult(401);
                return;
            }

            var authToken = authHeaders.AuthToken;
            var discoveredUser = (
                    from user in dbContext.User
                    where user.AuthToken == authToken && (user.WorldId == null || user.WorldId == world.Id)
                    select user
                ).FirstOrDefault();

            var allUsers = dbContext.User.ToList();

            if (discoveredUser == null || !discoveredUser.Enabled)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                if (discoveredUser == null)
                    failedRequest.Reason = $"No user found with given token: '{authToken}'";
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
                failedRequest.Reason = $"Player ID {authHeaders.PlayerId.Value} did not match the player ID associated with for token, got PID {discoveredUser.PlayerId}";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();
                context.Result = new StatusCodeResult(401);
                return;
            }

            if (discoveredUser.PermissionsLevel < Configuration.Security.MinimumRequiredPriveleges)
            {
                var failedRequest = AuthenticationUtil.MakeFailedRecord(context.HttpContext, authHeaders);
                failedRequest.Reason = $"User privileges '{discoveredUser.PermissionsLevel}' was less than the minimum '{Configuration.Security.MinimumRequiredPriveleges}'";

                dbContext.Add(failedRequest);
                dbContext.SaveChanges();
                context.Result = new StatusCodeResult(401);
                return;
            }


            context.HttpContext.Items["TribeId"] = authHeaders.TribeId.Value;
            context.HttpContext.Items["User"] = discoveredUser;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Profiling.StoreData(dbContext).Wait();
            base.OnActionExecuted(context);
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
