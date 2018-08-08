using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scaffold = TW.Vault.Scaffold_Model;

namespace TW.Vault.Security
{
    public static class AuthenticationUtil
    {
        public static AuthHeaders ParseHeaders(IHeaderDictionary headers)
        {
            var result = new AuthHeaders();

            var playerIdString = headers["X-V-PID"].FirstOrDefault();
            var tribeIdString = headers["X-V-TID"].FirstOrDefault();
            var authTokenString = headers["X-V-TOKEN"].FirstOrDefault();

            playerIdString = String.IsNullOrWhiteSpace(playerIdString) ? null : playerIdString;
            tribeIdString = String.IsNullOrWhiteSpace(tribeIdString) ? null : tribeIdString;
            authTokenString = String.IsNullOrWhiteSpace(authTokenString) ? null : authTokenString;

            long playerId, tribeId;
            if (long.TryParse(playerIdString, out playerId))
                result.PlayerId = playerId;
            if (long.TryParse(tribeIdString, out tribeId))
                result.TribeId = tribeId;

            if (authTokenString != null)
            {
                try
                {
                    result.AuthToken = Guid.Parse(authTokenString);
                } catch (Exception) { }
            }

            return result;
        }

        public static Scaffold.FailedAuthorizationRecord MakeFailedRecord(HttpContext httpContext, AuthHeaders headers)
        {
            var ip = httpContext.Connection.RemoteIpAddress;

            return new Scaffold.FailedAuthorizationRecord
            {
                Ip = ip,
                OccurredAt = DateTime.UtcNow,
                PlayerId = headers?.PlayerId,
                TribeId = headers?.TribeId,
                RequestedEndpoint = httpContext.Request.Path.Value
            };
        }

        public static PermissionLevel GetPermissionLevel(int permissionLevel)
        {
            PermissionLevel result;
            if (permissionLevel >= (int)PermissionLevel.System)
                result = PermissionLevel.System;
            else
            {
                switch (permissionLevel)
                {
                    case 1: result = PermissionLevel.Default; break;
                    case 2: result = PermissionLevel.Admin; break;

                    default: result = PermissionLevel.Default; break;
                }
            }

            return result;
        }
    }
}
