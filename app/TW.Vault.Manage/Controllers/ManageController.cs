using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TW.Vault.Lib;

namespace TW.Vault.Manage.Controllers
{
    [ApiController]
    public class ManageController : ControllerBase
    {
        readonly Lib.Scaffold.VaultContext context;
        readonly ILogger logger;

        private bool useCaptcha => Lib.Configuration.Instance["UseCaptcha"] == "true";

        private static readonly HttpClient client = new();
        public ManageController(Lib.Scaffold.VaultContext context, ILoggerFactory factory)
        {
            this.context = context;
            logger = factory.CreateLogger<ManageController>();
        }

        public class WorldInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [HttpGet("/use-captcha")]
        public ActionResult UseCaptcha() => Ok(useCaptcha);

        [HttpGet("/captcha-sitekey")]
        public ActionResult GetCaptchaInfo()
        {
            return Ok(Lib.Configuration.Instance["CaptchaSiteKey"]);
        }

        [HttpGet("/servers")]
        public ActionResult<IEnumerable<WorldInfo>> GetServers()
        {
            var worlds = context.World.ToList();

            return Ok(worlds.Where(w => !w.IsBeta).Select(w => new WorldInfo
            {
                Id = w.Id,
                Name = w.Hostname
            }).OrderBy(w => w.Name));
        }

        public class UserInfo
        {
            public int WorldId { get; set; }
            public string Name { get; set; }
            public string CaptchaToken { get; set; }
        }

        // POST api/values
        [HttpPost("/user")]
        public async Task<ActionResult> Post([FromBody] UserInfo userInfo)
        {
            var world = context.World.Where(w => w.Id == userInfo.WorldId).SingleOrDefault();
            if (world == null)
                return Ok(new { error = "No world exists with that id" });

            var encodedPlayerName = userInfo.Name.UrlEncode();
            var player = context.Player.Where(p => p.WorldId == userInfo.WorldId && p.PlayerName == encodedPlayerName).SingleOrDefault();
            if (player == null)
                return Ok(new { error = $"No user exists with that name on world {world.Hostname} (Name is Case-Sensitive).\n\nIf you just registered, you'll need to wait up to 60 minutes for the world data to refresh." });

            var remoteIp = HttpContext.Connection.RemoteIpAddress;

            if (useCaptcha)
            {
                try
                {
                    var captchaPrivateKey = Configuration.Instance["CaptchaSecretKey"];
                    var captchaUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={captchaPrivateKey}&response={userInfo.CaptchaToken}&remoteip={remoteIp}";
                    var captchaRequest = await client.GetAsync(captchaUrl);
                    var responseObject = JObject.Parse(await captchaRequest.Content.ReadAsStringAsync());

                    var success = responseObject.Value<bool>("success");
                    if (!success)
                    {
                        var errorCodes = responseObject.GetValue("error-codes").Values<string>();
                        logger.LogWarning("Got error codes from captcha when verifying for remote IP {0}: [{1}]", remoteIp, string.Join(", ", errorCodes));
                        return Ok(new { error = "Captcha verification failed" });
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Captcha error occured: {ex}", e);
                    return Ok(new { error = "An error occurred while verifying captcha" });
                }
            }

            var tx = new Lib.Scaffold.Transaction
            {
                OccurredAt = DateTime.UtcNow,
                WorldId = world.Id,
                Ip = remoteIp
            };
            context.Add(tx);

            var accessGroup = new Lib.Scaffold.AccessGroup
            {
                WorldId = userInfo.WorldId,
                Label = userInfo.Name
            };
            context.AccessGroup.Add(accessGroup);
            context.SaveChanges();

            var authToken = Guid.NewGuid();
            var user = new Lib.Scaffold.User
            {
                AccessGroupId = accessGroup.Id,
                WorldId = (short)userInfo.WorldId,
                PlayerId = player.PlayerId,
                Enabled = true,
                PermissionsLevel = 2,
                AuthToken = authToken,
                TransactionTime = DateTime.UtcNow,
                Label = $"{world.Name} - {userInfo.Name}",
                Tx = tx
            };

            context.User.Add(user);

            context.SaveChanges();

            logger.LogInformation("Creating user for {0} on world {1} from {2}", player.PlayerName, world.Hostname, remoteIp);

            return Ok(new { token = authToken.ToString() });
        }
    }
}
