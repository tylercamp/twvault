using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold;
using TW.Vault.Security;

namespace TW.Vault.Controllers
{
    public class AccessGroupRequest
    {
        public int? FirstPlayerId { get; set; }
        public String FirstPlayerName { get; set; }
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
        public ServerController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        [HttpGet("utc")]
        public IActionResult GetCurrentUtcTime() => Ok(new { UtcTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() });

        [HttpGet("time")]
        public IActionResult GetCurrentServerTime()
        {
            if (!PreloadWorldData())
                return NotFound();
            else
                return Ok(new { TwTime = CurrentServerTime });
        }

        [HttpGet("settings")]
        public IActionResult GetCurrentServerSettings()
        {
            if (!PreloadWorldData())
                return NotFound();
            else
                return Ok(new
                {
                    CurrentWorldSettings.ArchersEnabled,
                    CurrentWorldSettings.PaladinEnabled,
                    CurrentWorldSettings.MilitiaEnabled,
                    CurrentWorldSettings.WatchtowerEnabled,
                    CurrentWorldSettings.GameSpeed,
                    CurrentWorldSettings.UnitSpeed,
                    UtcOffset = DateTime.UtcNow - CurrentWorldSettings.ServerTime,
                    CurrentWorldSettings.MoraleEnabled,
                    CurrentWorldSettings.ChurchesEnabled,
                    CurrentWorldSettings.BonusVillagesEnabled,
                    CurrentWorldSettings.FlagsEnabled,
                    CurrentWorldSettings.NightBonusEnabled
                });
        }
        
        [HttpPost("access-group/{systemToken}")]
        public IActionResult CreateAccessGroup(String systemToken, [FromBody] AccessGroupRequest groupRequest)
        {
            var token = Guid.Parse(systemToken);
            var user = context.User.Where(u => u.AuthToken == token).FirstOrDefault();
            if (user == null || user.PermissionsLevel < (short)PermissionLevel.System)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (groupRequest.FirstPlayerId == null && groupRequest.FirstPlayerName == null)
                return BadRequest("Need to define at least either firstPlayerId or firstPlayerName");

            var escapedName = groupRequest.FirstPlayerName?.UrlEncode();
            var playerId = groupRequest.FirstPlayerId ?? context.Player.FromWorld(CurrentWorldId).Where(p => p.PlayerName == escapedName).First().PlayerId;

            var newGroup = new Scaffold.AccessGroup();
            newGroup.WorldId = CurrentWorldId;
            context.Add(newGroup);
            context.SaveChanges();

            var newUser = new Scaffold.User();
            newUser.TransactionTime = DateTime.UtcNow;
            newUser.AccessGroupId = newGroup.Id;
            newUser.AuthToken = Guid.NewGuid();
            newUser.PlayerId = playerId;
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