using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib.Security
{
    public static class DecryptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseVaultContentDecryption(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DecryptionMiddleware>();
        }
    }
}
