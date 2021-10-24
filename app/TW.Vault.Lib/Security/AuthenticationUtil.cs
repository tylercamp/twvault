using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Features;

namespace TW.Vault.Lib.Security
{
    public static class AuthenticationUtil
    {
        public static AuthHeaders ParseHeaders(IHeaderDictionary headers)
        {
            var result = new AuthHeaders();

            if (Configuration.Security.ForcedKey != null && Configuration.Security.ForcedPlayerId != null && Configuration.Security.ForcedTribeId != null)
            {
                result.AuthToken = Configuration.Security.ForcedKey;
                result.PlayerId = Configuration.Security.ForcedPlayerId;
                result.TribeId = Configuration.Security.ForcedTribeId;
                result.IsSitter = false;
                return result;
            }

            var combinedTokenString = headers["X-V-TOKEN"].FirstOrDefault();
            combinedTokenString = String.IsNullOrWhiteSpace(combinedTokenString) ? null : combinedTokenString;

            if (combinedTokenString != null)
            {
                combinedTokenString = Encryption.Decrypt(combinedTokenString);
                var tokenParts = combinedTokenString.Split(':');

                var playerIdString = tokenParts.Length > 0 ? tokenParts[0] : null;
                var tribeIdString = tokenParts.Length > 1 ? tokenParts[1] : null;
                var authTokenString = tokenParts.Length > 2 ? tokenParts[2] : null;
                var sitterString = tokenParts.Length > 3 ? tokenParts[3] : null;

                int playerId, tribeId;
                if (int.TryParse(playerIdString, out playerId))
                    result.PlayerId = playerId;
                if (int.TryParse(tribeIdString, out tribeId))
                    result.TribeId = tribeId;

                bool isSitter;
                if (bool.TryParse(sitterString, out isSitter))
                    result.IsSitter = isSitter;
                else
                    result.IsSitter = true;


                try
                {
                    result.AuthToken = Guid.Parse(authTokenString);
                }
                catch { }
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
