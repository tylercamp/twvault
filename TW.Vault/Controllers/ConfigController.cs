using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Config")]
    public class ConfigController : Controller
    {
        Scaffold.VaultContext context;

        public ConfigController(Scaffold.VaultContext context)
        {
            this.context = context;
        }

        [HttpGet("{authKey}")]
        public async Task<IActionResult> GetCurrentConfig(String authKey)
        {
            Guid key;
            try
            {
                key = Guid.Parse(authKey);
            }
            catch
            {
                return NotFound();
            }

            var user = await context.User.FirstOrDefaultAsync(u => u.AuthToken == key);
            if (user == null || user.PermissionsLevel < (int)Security.PermissionLevel.System)
            {
                return NotFound();
            }

            var config = Configuration.Instance;
            var configDictionary = new Dictionary<String, String>();
            foreach (var section in config.GetChildren())
                AttachConfigValues(section, configDictionary);

            var json = JsonConvert.SerializeObject(configDictionary, Formatting.Indented);
            return Content(json);
        }

        private static void AttachConfigValues(IConfigurationSection section, Dictionary<String, String> target)
        {
            if (!section.GetChildren().Any())
            {
                target[section.Path] = section.Value;
            }
            else
            {
                foreach (var child in section.GetChildren())
                    AttachConfigValues(child, target);
            }
        }
    }
}