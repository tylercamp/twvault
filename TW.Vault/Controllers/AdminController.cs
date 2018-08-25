using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Native;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Admin")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class AdminController : BaseController
    {
        public AdminController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        [HttpGet]
        public object CheckIsAdmin()
        {
            return new { isAdmin = CurrentUserIsAdmin };
        }

        [HttpGet("keys")]
        public async Task<IActionResult> GetVaultKeys()
        {
            if (!CurrentUserIsAdmin)
                return Unauthorized();

            var users = await (
                    from user in context.User
                    join player in context.Player on user.PlayerId equals player.PlayerId
                    join tribe in context.Ally on player.TribeId equals tribe.TribeId
                    where CurrentUser.KeySource == null || user.KeySource == CurrentUser.Uid
                    where user.Enabled
                    where player.WorldId == CurrentWorldId
                    where (user.PermissionsLevel < (short)Security.PermissionLevel.System) || CurrentUserIsSystem
                    select new { user, playerName = player.PlayerName, tribeName = tribe.TribeName }
                ).ToListAsync();

            var jsonUsers = users.Select(p => UserConvert.ModelToJson(
                p.user,
                WebUtility.UrlDecode(p.playerName),
                WebUtility.UrlDecode(p.tribeName)
            ));

            return Ok(jsonUsers);
        }

        [HttpPost("keys")]
        public async Task<IActionResult> MakeVaultKey([FromBody]JSON.VaultKeyRequest keyRequest)
        {
            if (!CurrentUserIsAdmin)
                return Unauthorized();

            Scaffold.Player player;
            if (keyRequest.PlayerId.HasValue)
            {
                long playerId = keyRequest.PlayerId.Value;
                var possiblePlayer = await (
                        from p in context.Player.FromWorld(CurrentWorldId)
                        where p.PlayerId == playerId
                        select p
                    ).FirstOrDefaultAsync();

                if (possiblePlayer == null)
                {
                    return BadRequest(new { error = "No player could be found with the given player ID." });
                }

                player = possiblePlayer;
            }
            else if (keyRequest.PlayerName != null)
            {
                var formattedPlayerName = WebUtility.UrlEncode(keyRequest.PlayerName);

                var possiblePlayer = await (
                        from p in context.Player.FromWorld(CurrentWorldId)
                        where p.PlayerName == formattedPlayerName
                        select p
                    ).FirstOrDefaultAsync();

                if (possiblePlayer == null)
                {
                    return BadRequest(new { error = "No user could be found with the given name." });
                }

                player = possiblePlayer;
            }
            else
            {
                return BadRequest(new { error = "Either the player ID or player name must be specified." });
            }

            if (!CurrentUserIsSystem && player.TribeId != CurrentTribeId)
            {
                return BadRequest(new { error = "Cannot request a key for a player that's not in your tribe." });
            }

            bool userExists = await (
                    from user in context.User
                    where user.PlayerId == player.PlayerId
                    where user.WorldId == null || user.WorldId == CurrentWorldId
                    where user.Enabled
                    select user
                ).AnyAsync();

            if (userExists)
            {
                return BadRequest(new { error = "This user already has an auth key." });
            }

            var newAuthUser = new Scaffold.User();
            newAuthUser.WorldId = CurrentWorldId;
            newAuthUser.PlayerId = player.PlayerId;
            newAuthUser.AuthToken = Guid.NewGuid();
            newAuthUser.Enabled = true;
            newAuthUser.TransactionTime = DateTime.UtcNow;
            newAuthUser.AdminAuthToken = CurrentUser.AuthToken;
            newAuthUser.AdminPlayerId = CurrentUser.PlayerId;
            newAuthUser.KeySource = CurrentUser.Uid;

            if (keyRequest.NewUserIsAdmin)
                newAuthUser.PermissionsLevel = (short)Security.PermissionLevel.Admin;
            else
                newAuthUser.PermissionsLevel = (short)Security.PermissionLevel.Default;

            context.User.Add(newAuthUser);
            await context.SaveChangesAsync();

            var jsonUser = UserConvert.ModelToJson(newAuthUser);
            jsonUser.PlayerName = WebUtility.UrlDecode(player.PlayerName);

            var playerTribe = await (
                    from tribe in context.Ally.FromWorld(CurrentWorldId)
                    where tribe.TribeId == player.TribeId
                    select tribe
                ).FirstOrDefaultAsync();

            jsonUser.TribeName = WebUtility.UrlDecode(playerTribe.TribeName);

            return Ok(jsonUser);
        }

        [HttpDelete("keys/{authKeyString}")]
        public async Task<IActionResult> RevokeKey(String authKeyString)
        {
            if (!CurrentUserIsAdmin)
                return Unauthorized();

            Guid authKey;
            try
            {
                authKey = Guid.Parse(authKeyString);
            }
            catch
            {
                return BadRequest(new { error = "Invalid auth key." });
            }

            var requestedUser = await (
                    from u in context.User
                    where u.AuthToken == authKey
                    select u
                ).FirstOrDefaultAsync();

            if (requestedUser == null)
            {
                return BadRequest(new { error = "No user exists with that auth key." });
            }

            if (requestedUser.AuthToken == CurrentUser.AuthToken)
            {
                return BadRequest(new { error = "You cannot delete your own key." });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System)
            {
                return BadRequest(new { error = "You cannot delete a system token." });
            }

            if (requestedUser.PermissionsLevel == (short)Security.PermissionLevel.Admin)
            {
                if (!CurrentUserIsSystem && requestedUser.KeySource.HasValue && requestedUser.KeySource.Value != CurrentUser.Uid)
                {
                    return BadRequest(new { error = "You cannot delete an admin user that you have not created." });
                }
            }

            logger.LogWarning("User {SourceKey} disabling {TargetKey}", CurrentUser.AuthToken, authKey);
            requestedUser.Enabled = false;
            requestedUser.AdminAuthToken = CurrentUser.AuthToken;
            requestedUser.AdminPlayerId = CurrentUser.PlayerId;
            requestedUser.TransactionTime = DateTime.UtcNow;

            context.User.Update(requestedUser);
            await context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("keys/{authKeyString}/setAdmin")]
        public async Task<IActionResult> SetKeyAdmin(String authKeyString, [FromBody]JSON.UpdateAdminKeyRequest updateRequest)
        {
            if (!CurrentUserIsAdmin)
                return Unauthorized();

            //  WARNING - Copy/pasted auth check from RevokeKey!
            Guid authKey;
            try
            {
                authKey = Guid.Parse(authKeyString);
            }
            catch
            {
                return BadRequest(new { error = "Invalid auth key." });
            }

            var requestedUser = await (
                    from u in context.User
                    where u.AuthToken == authKey
                    select u
                ).FirstOrDefaultAsync();

            if (requestedUser == null)
            {
                return BadRequest(new { error = "No user exists with that auth key." });
            }

            if (requestedUser.AuthToken == CurrentUser.AuthToken)
            {
                return BadRequest(new { error = "You cannot change admin status of your own key." });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System)
            {
                return BadRequest(new { error = "You cannot change admin status of a system key." });
            }

            if (requestedUser.PermissionsLevel == (short)Security.PermissionLevel.Admin)
            {
                if (!CurrentUserIsSystem && requestedUser.KeySource.HasValue && requestedUser.KeySource.Value != CurrentUser.Uid)
                {
                    return BadRequest(new { error = "You cannot change the admin status of an admin that you have not created." });
                }
            }

            throw new NotImplementedException();
        }


        [HttpGet("summary")]
        public async Task<IActionResult> GetTroopsSummary()
        {
            //  TODO
            if (!CurrentUserIsAdmin)
                return Unauthorized();

            var tribeVillages = await (
                    from player in context.Player.FromWorld(CurrentWorldId)
                    join village in context.Village on player.PlayerId equals village.PlayerId
                    join currentVillage in context.CurrentVillage
                                                  .Include(v => v.ArmyOwned)
                                        on village.VillageId equals currentVillage.VillageId
                    where player.TribeId == CurrentTribeId
                    select new { player, currentVillage }
                ).ToListAsync();

            var currentPlayers = await (
                    from currentPlayer in context.CurrentPlayer.FromWorld(CurrentWorldId)
                    join player in context.Player on currentPlayer.PlayerId equals player.PlayerId
                    where player.TribeId == CurrentTribeId
                    select currentPlayer
                ).ToListAsync();

            var villagesByPlayer = tribeVillages
                                        .Select(v => v.player)
                                        .Distinct()
                                        .ToDictionary(
                                            p => p,
                                            p => tribeVillages.Where(v => v.player == p)
                                                              .Select(tv => tv.currentVillage)
                                                              .ToList()
                                         );

            var maxNoblesByPlayer = currentPlayers.ToDictionary(p => p.PlayerId, p => p.CurrentPossibleNobles);

            var jsonData = new List<JSON.PlayerSummary>();
            foreach (var kvp in villagesByPlayer)
            {
                String playerName = kvp.Key.PlayerName;
                var villages = kvp.Value;

                var playerSummary = new JSON.PlayerSummary();
                playerSummary.PlayerName = WebUtility.UrlDecode(playerName);
                playerSummary.PlayerId = kvp.Key.PlayerId;
                playerSummary.Armies = new List<JSON.Army>();

                if (maxNoblesByPlayer.ContainsKey(kvp.Key.PlayerId))
                    playerSummary.MaxPossibleNobles = maxNoblesByPlayer[kvp.Key.PlayerId];

                var oldestUpload = villages.Select(v => v.ArmyOwned.LastUpdated).Min();
                if (oldestUpload != null)
                {
                    playerSummary.UploadedAt = oldestUpload.Value;
                    playerSummary.UploadAge = DateTime.UtcNow - oldestUpload.Value;
                }
                else
                {
                    playerSummary.UploadedAt = new DateTime();
                    playerSummary.UploadAge = DateTime.UtcNow - playerSummary.UploadedAt;
                }

                foreach (var village in villages)
                    playerSummary.Armies.Add(ArmyConvert.ArmyToJson(village.ArmyOwned));

                jsonData.Add(playerSummary);
            }

            return Ok(jsonData);
        }

        [HttpGet("op-plan")]
        public async Task<IActionResult> GenerateOpPlan()
        {
            //  TODO
            return NotFound();
        }
    }
}
