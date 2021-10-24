using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TW.Vault.Lib.Features;

namespace TW.Vault.Lib.Security
{
    public class DecryptionMiddleware
    {
        private RequestDelegate nextDelegate;

        public DecryptionMiddleware(RequestDelegate nextDelegate)
        {
            this.nextDelegate = nextDelegate;
        }

        public async Task InvokeAsync(HttpContext context, Scaffold.VaultContext vaultContext)
        {
            var bodyContents = await new StreamReader(context.Request.Body).ReadToEndAsync();

            if (!String.IsNullOrWhiteSpace(bodyContents))
            {
                try
                {
                    string unencrypted = Encryption.Decrypt(bodyContents);
                    context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(unencrypted));
                }
                catch (InvalidStringEncryptionException)
                {
                    //  TODO
                    //  
                    //  This will require setting UserId nullable in InvalidDataRecord
                    //  Should also record IP

                    //vaultContext.Add(new Scaffold.InvalidDataRecord
                    //{
                    //    Reason = ex.Message,
                    //    DataString = bodyContents,
                    //    Endpoint = context.Request.Path.ToString(),
                        
                    //});
                }
            }

            await nextDelegate(context);
        }

        private AuthHeaders GetAuthHeaders(HttpContext context)
        {
            try
            {
                return AuthenticationUtil.ParseHeaders(context.Request.Headers);
            }
            catch
            {
                return null;
            }
        }
    }
}
