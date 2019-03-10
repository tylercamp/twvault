using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    public class AccessGroupRequest
    {
        public int FirstPlayerId { get; set; }
        public short PermissionsLevel { get; set; }
    }

    //  Retrives current UTC time for synchronizing script with encryption swap interval
    //  (Don't rely on user's UTC time on their machine, TW server time can have variable
    //  offsets from UTC time depending on server)
    [Produces("application/json")]
    [Route("api/{worldName}/Server")]
    [EnableCors("AllOrigins")]
    public class ServerController : BaseController
    {
        public ServerController(VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        [HttpGet("utc")]
        public IActionResult GetCurrentUtcTime() => Ok(new { UtcTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() });

        [HttpGet("time")]
        public IActionResult GetCurrentServerTime() => Ok(new { TwTime = CurrentServerTime });

        [HttpGet("settings")]
        public IActionResult GetCurrentServerSettings() => Ok(new {
            CurrentWorldSettings.ArchersEnabled,
            CurrentWorldSettings.PaladinEnabled,
            CurrentWorldSettings.MilitiaEnabled,
            CurrentWorldSettings.WatchtowerEnabled,
            CurrentWorldSettings.GameSpeed,
            CurrentWorldSettings.UnitSpeed,
            CurrentWorldSettings.UtcOffset,
            CurrentWorldSettings.MoraleEnabled,
            CurrentWorldSettings.ChurchesEnabled,
            CurrentWorldSettings.BonusVillagesEnabled,
            CurrentWorldSettings.FlagsEnabled,
            CurrentWorldSettings.NightBonusEnabled
        });

        [HttpPost("access-group/{systemToken}")]
        public IActionResult CreateAccessGroup(String systemToken, [FromBody] AccessGroupRequest groupRequest)
        {
            var token = Guid.Parse(systemToken);
            var user = context.User.Where(u => u.AuthToken == token).FirstOrDefault();
            if (user == null || user.PermissionsLevel < (short)PermissionLevel.System)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newGroup = new Scaffold.AccessGroup();
            newGroup.WorldId = CurrentWorldId;
            context.Add(newGroup);
            context.SaveChanges();

            var newUser = new Scaffold.User();
            newUser.TransactionTime = DateTime.UtcNow;
            newUser.AccessGroupId = newGroup.Id;
            newUser.AuthToken = Guid.NewGuid();
            newUser.PlayerId = groupRequest.FirstPlayerId;
            newUser.PermissionsLevel = groupRequest.PermissionsLevel;
            newUser.Enabled = true;
            newUser.IsReadOnly = false;
            newUser.WorldId = CurrentWorldId;
            context.Add(newUser);
            context.SaveChanges();

            return Ok(new
            {
                AuthToken = newUser.AuthToken,
                Script = $"javascript:window.vaultToken='{newUser.AuthToken}';$.getScript('https://v.tylercamp.me/script/main.js')"
            });
        }
    }
}