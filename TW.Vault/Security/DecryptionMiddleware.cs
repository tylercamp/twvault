using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TW.Vault.Features;

namespace TW.Vault.Security
{
    public class DecryptionMiddleware
    {
        private RequestDelegate nextDelegate;

        public DecryptionMiddleware(RequestDelegate nextDelegate)
        {
            this.nextDelegate = nextDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var bodyContents = new StreamReader(context.Request.Body).ReadToEnd();

            if (!String.IsNullOrWhiteSpace(bodyContents))
            {
                //  Try decrypting using all available active seeds
                string unencrypted = Encryption.Decrypt(bodyContents);
                context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(unencrypted));
            }

            await nextDelegate(context);
        }
    }
}
