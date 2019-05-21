using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Caching;
using TW.Vault.Scaffold;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    [Route("script")]
    [EnableCors("AllOrigins")]
    public class ScriptController : BaseController
    {
        ASPUtil asputil;

        private const String ScriptsBasePath = "";

        public ScriptController(IHostingEnvironment environment, IServiceScopeFactory scopeFactory, VaultContext context, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
            asputil = new ASPUtil(environment, ScriptsBasePath);
        }

        private static AsyncCachingMap<String> CachedFakeScripts = new AsyncCachingMap<String> { MaxCacheAge = TimeSpan.FromMinutes(5) };

        [HttpGet("fake.js")]
        public async Task<IActionResult> GetFakeScript()
        {
            IEnumerable<String> TrimAndFilter(IEnumerable<String> e) => e.Select(s => s.Trim()).Where(s => !String.IsNullOrWhiteSpace(s)).OrderBy(s => s);

            var players = TrimAndFilter(Request.Query["player"].ToString().Split(',')).ToList();
            var tribes = TrimAndFilter(Request.Query["tribe"].ToString().Split(',')).ToList();
            var continents = TrimAndFilter(Request.Query["k"].ToString().Split(',')).Where(k => k.Length == 2).ToList();
            var server = Request.Query["server"].ToString();
            var minCoord = Request.Query["min"].ToString();
            var maxCoord = Request.Query["max"].ToString();
            var center = Request.Query["center"].ToString();

            players = players.Select(MiscExtensions.UrlEncode).ToList();
            tribes = tribes.Select(MiscExtensions.UrlEncode).ToList();

            var scriptId = $"player_{string.Join('_', players)}__tribe_{string.Join('_',tribes)}__k_{string.Join('_',continents)}__server_{server}";
            scriptId += $"__min_{minCoord}__max_{maxCoord}__center_{center}";

            scriptId = Regex.Replace(scriptId, @"[^\w\d_]", "_");

            var fakeScript = await CachedFakeScripts.GetOrPopulate(scriptId, context, async (ctx) =>
            {
                var context = ctx as VaultContext;

                logger.LogInformation("Regenerating fake script for {0}", scriptId);

                var worldId = (await context.World.FirstOrDefaultAsync(w => w.Hostname == server))?.Id;
                if (worldId == null)
                    return null;

                var query = new Features.VillageSearch.Query
                {
                    WorldId = worldId.Value,
                    PlayerNames = players,
                    TribeNamesOrTags = tribes,
                    Continents = continents
                };

                if (!string.IsNullOrWhiteSpace(minCoord))
                {
                    var minParts = minCoord.Split('|');
                    query.MinCoord = new Model.Coordinate { X = int.Parse(minParts[0]), Y = int.Parse(minParts[1]) };
                }

                if (!string.IsNullOrWhiteSpace(maxCoord))
                {
                    var maxParts = maxCoord.Split('|');
                    query.MaxCoord = new Model.Coordinate { X = int.Parse(maxParts[0]), Y = int.Parse(maxParts[1]) };
                }

                if (!string.IsNullOrWhiteSpace(center))
                {
                    var centerParts = center.Split('|');
                    query.CenterCoord = new Model.Coordinate { X = int.Parse(centerParts[0]), Y = int.Parse(centerParts[1]) };
                    query.MaxDistance = float.Parse(centerParts[2]);
                }

                var coordString = await Features.VillageSearch.ListCoords(context, query);

                var baseScript = GetFileContents("fakes.js");

                var formattedScript = baseScript
                    .Replace("{TARGET_COORDS}", coordString)
                    .Replace("{COOKIE}", scriptId);

                return formattedScript;
            });

            if (fakeScript == null)
                return NotFound();
            else
                return Content(fakeScript, "application/javascript");
        }
        
        [HttpGet("{*name}", Name = "GetCompiledObfuscatedScript")]
        public IActionResult GetCompiledObfuscated(String name)
        {
            var allowedPublicScripts = Configuration.Security.PublicScripts;
            if (Configuration.Security.EnableScriptFilter)
            {
                if (allowedPublicScripts.Contains(name))
                    return Content(ResolveFileContents(name), "application/json");
                else
                    return NotFound();
            }

            String errorString = null, notFoundString = null;
            String scriptContents = MakeCompiled(name, (e) => errorString = e, (n) => notFoundString = n);

            if (errorString != null)
                return StatusCode(500, errorString);

            if (notFoundString != null)
                return NotFound();

            //var minified = Uglify.Js(scriptContents).Code;
            //return Content(minified, "application/json");
            return Content(scriptContents, "application/javascript");
        }

        [HttpGet("real/{authToken}/{*name}", Name = "GetCompiledUnobfuscatedScript")]
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
        [HttpGet("raw/{authToken}/{*name}", Name = "GetRawUnobfuscatedScript")]
        public IActionResult GetRaw(String authToken, String name)
        {
            String contents = ResolveFileContents(name);
            if (contents == null)
                return NotFound();
            else
                return Content(contents, "application/javascript");
        }

        private String GetFileContents(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            var resolvedPath = asputil.GetFilePath(name);

            if (resolvedPath != null && System.IO.File.Exists(resolvedPath))
                return System.IO.File.ReadAllText(resolvedPath);
            else
                return null;
        }
        
        private String ResolveFileContents(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            if (Configuration.Initialization.EnableRequiredFiles)
            {
                foreach (var externalFile in Configuration.Initialization.RequiredFiles)
                {
                    var fileName = Path.GetFileName(externalFile);
                    if (name == fileName)
                        return System.IO.File.ReadAllText(Path.GetFullPath(Path.Combine(asputil.HostingEnvironment.WebRootPath, externalFile)));
                }

                throw new FileNotFoundException("Could not find required file: " + name);
            }

            var resolvedPath = asputil.GetFilePath(name);

            if (System.IO.File.Exists(resolvedPath))
                return System.IO.File.ReadAllText(resolvedPath);
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
                String contents = ResolveFileContents(fileName);
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
