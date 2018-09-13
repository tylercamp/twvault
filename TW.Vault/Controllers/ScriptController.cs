using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUglify;
using TW.Vault.Scaffold;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    [Route("script")]
    [EnableCors("AllOrigins")]
    public class ScriptController : BaseController
    {
        IHostingEnvironment environment;

        private const String ScriptsBasePath = "";

        public ScriptController(IHostingEnvironment environment, VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
            this.environment = environment;
        }
        
        [HttpGet("{name}", Name = "GetCompiledObfuscatedScript")]
        public IActionResult GetCompiledObfuscated(String name)
        {
            var allowedPublicScripts = Configuration.Security.PublicScripts;
            if (Configuration.Security.EnableScriptFilter && !allowedPublicScripts.Contains(name))
                return NotFound();

            String errorString = null, notFoundString = null;
            String scriptContents = MakeCompiled(name, (e) => errorString = e, (n) => notFoundString = n);

            if (errorString != null)
                return StatusCode(500, errorString);

            if (notFoundString != null)
                return NotFound();

            //var minified = Uglify.Js(scriptContents).Code;
            //return Content(minified, "application/json");
            return Content(scriptContents, "application/json");
        }

        [HttpGet("real/{authToken}/{name}", Name = "GetCompiledUnobfuscatedScript")]
        public async Task<IActionResult> GetCompiledUnobfuscated(String authToken, String name)
        {
            Guid authGuid;
            try
            {
                authGuid = Guid.Parse(authToken);
            }
            catch
            {
                //  Hide endpoint
                var authRecord = MakeFailedAuthRecord("Invalid auth token " + authToken);
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return NotFound();
            }

            var user = await (
                    from u in context.User
                    where u.AuthToken == authGuid
                    select u
                ).FirstOrDefaultAsync();

            if (user == null)
            {
                var authRecord = MakeFailedAuthRecord("User does not exist with auth token " + authToken);
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return NotFound();
            }

            if (user.PermissionsLevel < (short)PermissionLevel.System)
            {
                var authRecord = MakeFailedAuthRecord("User is not System with auth token " + authToken);
                context.Add(authRecord);
                await context.SaveChangesAsync();

                //  Hide the endpoint
                return NotFound();
            }

            String errorString = null, notFoundString = null;
            String scriptContents = MakeCompiled(name, (e) => errorString = e, (n) => notFoundString = n);

            if (errorString != null)
                return StatusCode(500, errorString);

            if (notFoundString != null)
                return NotFound();

            return Content(scriptContents, "application/javascript");
        }
        
        // GET: raw/scriptName.js
        [HttpGet("raw/{authToken}/{name}", Name = "GetRawUnobfuscatedScript")]
        public IActionResult GetRaw(String authToken, String name)
        {
            String contents = GetFileContents(name);
            if (contents == null)
                return NotFound();
            else
                return Content(contents, "application/javascript");
        }
        
        private String GetFileContents(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            if (Configuration.Initialization.EnableRequiredFiles)
            {
                foreach (var externalFile in Configuration.Initialization.RequiredFiles)
                {
                    var fileName = Path.GetFileName(externalFile);
                    if (name == fileName)
                        return System.IO.File.ReadAllText(Path.GetFullPath(Path.Combine(environment.WebRootPath, externalFile)));
                }
            }

            String rootPath = Path.Combine(environment.WebRootPath, ScriptsBasePath);

            String fullPath = Path.Combine(rootPath, name);
            String absolutePath = Path.GetFullPath(fullPath);

            //  Prevent directory traversal
            if (!absolutePath.StartsWith(rootPath))
                return null;

            if (System.IO.File.Exists(fullPath))
                return System.IO.File.ReadAllText(fullPath);
            else
                return null;
        }

        private String MakeCompiled(String name, Action<String> onError, Action<String> onNotFound)
        {
            var scriptCompiler = new Features.ScriptCompiler();

            List<String> failedFiles = new List<string>();

            IEnumerable<String> circularDependencyChain = null;
            scriptCompiler.DependencyResolver = (fileName) =>
            {
                String contents = GetFileContents(fileName);
                if (contents == null)
                    failedFiles.Add(fileName);
                return contents;
            };
            scriptCompiler.OnCircularDependency += (chain) => circularDependencyChain = new[] { name }.Concat(chain);

            String scriptContents = scriptCompiler.Compile(name);

            if (circularDependencyChain != null)
            {
                onError?.Invoke("Error compiling script due to circular dependency: " + String.Join(" -> ", circularDependencyChain.ToArray()));
                return null;
            }

            if (failedFiles.Any())
            {
                if (failedFiles.Contains(name))
                    onNotFound?.Invoke(name);
                else
                    onError?.Invoke("Error compiling script since dependencies [" + String.Join(", ", failedFiles) + "] could not be resolved");

                return null;
            }

            String originalUrl = Request.Headers["X-Original-Url"].FirstOrDefault();
            if (originalUrl == null)
                originalUrl = Request.Path.Value;

            if (originalUrl.Contains("?"))
                originalUrl = originalUrl.Substring(0, originalUrl.IndexOf('?'));

            String sourceUrlLink = $"//# sourceURL=https://v.tylercamp.me{originalUrl}";
            scriptContents += ("\n" + sourceUrlLink);

            return scriptContents;
        }
    }
}
