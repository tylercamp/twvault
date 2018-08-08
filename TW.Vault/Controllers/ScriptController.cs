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
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold_Model;

namespace TW.Vault.Controllers
{
    [Route("script")]
    [EnableCors("AllOrigins")]
    public class ScriptController : ControllerBase
    {
        IHostingEnvironment environment;

        public ScriptController(IHostingEnvironment environment, VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
            this.environment = environment;
        }

        // GET: scriptName.js
        [HttpGet("{name}", Name = "GetCompiled")]
        public IActionResult GetCompiled(String name)
        {
            var scriptCompiler = new Features.ScriptCompiler();

            List<String> failedFiles = new List<string>();

            IEnumerable<String> circularDependencyChain = null;
            scriptCompiler.DependencyResolver = (fileName) =>
            {
                String contents = getFileContents(fileName);
                if (contents == null)
                    failedFiles.Add(fileName);
                return contents;
            };
            scriptCompiler.OnCircularDependency += (chain) => circularDependencyChain = new[] { name }.Concat(chain);

            String scriptContents = scriptCompiler.Compile(name);

            if (circularDependencyChain != null)
                return StatusCode(500, "Error compiling script due to circular dependency: " + String.Join(" -> ", circularDependencyChain.ToArray()));
            
            if (failedFiles.Any())
            {
                if (failedFiles.Contains(name))
                    return NotFound();
                else
                    return StatusCode(500, "Error compiling script since dependencies [" + String.Join(", ", failedFiles) + "] could not be resolved");
            }

            String originalUrl = Request.Headers["X-Original-Url"].FirstOrDefault();
            if (originalUrl == null)
                originalUrl = Request.Path.Value;

            if (originalUrl.Contains("?"))
                originalUrl = originalUrl.Substring(0, originalUrl.IndexOf('?'));

            String sourceUrlLink = $"//# sourceURL=https://v.tylercamp.me{originalUrl}";
            scriptContents += ("\n" + sourceUrlLink);

            return Content(scriptContents, "application/json");
        }
        
        // GET: raw/scriptName.js
        [HttpGet("raw/{name}")]
        public IActionResult GetRaw(String name)
        {
            String contents = getFileContents(name);
            if (contents == null)
                return NotFound();
            else
                return Content(contents);
        }
        
        private String getFileContents(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return null;

            String fullPath = Path.Combine(environment.WebRootPath, name);
            String absolutePath = Path.GetFullPath(fullPath);

            //  Prevent directory traversal
            if (!absolutePath.StartsWith(environment.WebRootPath))
                return null;

            if (System.IO.File.Exists(fullPath))
                return System.IO.File.ReadAllText(fullPath);
            else
                return null;
        }
    }
}
