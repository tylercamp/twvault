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
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            var users = await (
                    from user in context.User
                    join player in context.Player on user.PlayerId equals player.PlayerId
                    join tribe in context.Ally on player.TribeId equals tribe.TribeId
                    where CurrentUser.KeySource == null || user.KeySource == CurrentUser.Uid || !Configuration.Security.RestrictAccessWithinTribes
                    where user.Enabled
                    where player.WorldId == CurrentWorldId
                    where (user.PermissionsLevel < (short)Security.PermissionLevel.System) || CurrentUserIsSystem
                    orderby tribe.Tag, player.PlayerName
                    select new { user, playerName = player.PlayerName, tribeName = tribe.TribeName, tribeId = tribe.TribeId }
                ).ToListAsync();

            if (Configuration.Security.RestrictAccessWithinTribes && !CurrentUserIsSystem)
                users = users.Where(u => u.tribeId == CurrentTribeId).ToList();

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
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            if (!CurrentUserIsSystem && player.TribeId != CurrentTribeId && Configuration.Security.RestrictAccessWithinTribes)
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
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

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
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            //  This is a mess because of different classes for Player, CurrentPlayer, etc

            var (tribeVillages, currentPlayers, uploadHistory) = await ManyTasks.RunToList(
                //  Get all CurrentVillages from the user's tribe - list of (Player, CurrentVillage)
                from player in context.Player.FromWorld(CurrentWorldId)
                join user in context.User on player.PlayerId equals user.PlayerId
                join village in context.Village.FromWorld(CurrentWorldId) on player.PlayerId equals village.PlayerId
                join currentVillage in context.CurrentVillage.IncludeCurrentVillageData()
                                    on village.VillageId equals currentVillage.VillageId
                where user.Enabled
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select new { player, currentVillage }

                ,

                //  Get all CurrentPlayer data for the user's tribe (separate from global 'Player' table
                //      so we can also output stats for players that haven't uploaded anything yet)
                from currentPlayer in context.CurrentPlayer.FromWorld(CurrentWorldId)
                join player in context.Player on currentPlayer.PlayerId equals player.PlayerId
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select currentPlayer
                
                ,

                //  Get user upload history
                from history in context.UserUploadHistory
                join user in context.User on history.Uid equals user.Uid
                join player in context.Player.FromWorld(CurrentWorldId) on user.PlayerId equals player.PlayerId
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select new { playerId = player.PlayerId, history }
            );

            var tribeIds = tribeVillages.Select(tv => tv.player.TribeId)
                                        .Where(tid => tid != null)
                                        .Distinct()
                                        .Select(tid => tid.Value)
                                        .ToList();
            //  Get tribe labels
            var tribeNames = await (
                    from tribe in context.Ally.FromWorld(CurrentWorldId)
                    where tribeIds.Contains(tribe.TribeId)
                    select new { tribe.Tag, tribe.TribeId }
                ).ToListAsync();

            var tribeNamesById = tribeNames.ToDictionary(tn => tn.TribeId, tn => tn.Tag);

            //  Collect villages grouped by owner
            var villagesByPlayer = tribeVillages
                                        .Select(v => v.player)
                                        .Distinct()
                                        .ToDictionary(
                                            p => p,
                                            p => tribeVillages.Where(v => v.player == p)
                                                              .Select(tv => tv.currentVillage)
                                                              .ToList()
                                         );

            var uploadHistoryByPlayer = uploadHistory
                                        .Select(h => h.playerId)
                                        .Distinct()
                                        .ToDictionary(
                                            p => p,
                                            p => uploadHistory.Where(h => h.playerId == p)
                                                              .Select(h => h.history)
                                                              .FirstOrDefault()
                                        );

            //  Get all support data for the tribe
            var tribeVillageIds = tribeVillages.Select(v => v.currentVillage.VillageId).ToList();

            var villagesSupport = await (
                    from support in context.CurrentVillageSupport
                                           .FromWorld(CurrentWorldId)
                                           .Include(s => s.SupportingArmy)
                    where tribeVillageIds.Contains(support.SourceVillageId)
                    select support
                ).ToListAsync();


            //  Get support data by player Id
            var playersById = tribeVillages.Select(tv => tv.player).Distinct().ToDictionary(p => p.PlayerId, p => p);

            var villagesSupportByPlayerId = currentPlayers.ToDictionary(
                p => p.PlayerId,
                p => villagesSupport.Where(
                    s => villagesByPlayer[playersById[p.PlayerId]].Any(
                        v => v.VillageId == s.SourceVillageId
                    )
                )
            );

            var maxNoblesByPlayer = currentPlayers.ToDictionary(p => p.PlayerId, p => p.CurrentPossibleNobles);

            var jsonData = new List<JSON.PlayerSummary>();
            foreach (var kvp in villagesByPlayer.OrderBy(kvp => kvp.Key.TribeId).ThenBy(kvp => kvp.Key.PlayerName))
            {
                var player = kvp.Key;
                String playerName = player.PlayerName;
                String tribeName = tribeNamesById.GetValueOrDefault(player.TribeId ?? -1);
                var playerVillages = kvp.Value;

                var playerSummary = new JSON.PlayerSummary();
                playerSummary.PlayerName = WebUtility.UrlDecode(playerName);
                playerSummary.PlayerId = player.PlayerId;
                playerSummary.TribeName = tribeName;
                playerSummary.ArmiesOwned = new List<JSON.Army>();
                playerSummary.ArmiesTraveling = new List<JSON.Army>();

                if (maxNoblesByPlayer.ContainsKey(player.PlayerId))
                    playerSummary.MaxPossibleNobles = maxNoblesByPlayer[player.PlayerId];

                var playerHistory = uploadHistoryByPlayer.GetValueOrDefault(player.PlayerId);
                playerSummary.UploadedAt = playerHistory?.LastUploadedTroopsAt ?? new DateTime();
                playerSummary.UploadAge = DateTime.UtcNow - playerSummary.UploadedAt;
                playerSummary.UploadedReportsAt = playerHistory?.LastUploadedReportsAt ?? new DateTime();
                playerSummary.UploadedIncomingsAt = playerHistory?.LastUploadedIncomingsAt ?? new DateTime();
                playerSummary.UploadedCommandsAt = playerHistory?.LastUploadedCommandsAt ?? new DateTime();

                //  General army data
                foreach (var village in playerVillages)
                {
                    var armyTraveling = ArmyConvert.ArmyToJson(village.ArmyTraveling);

                    playerSummary.ArmiesOwned.Add(ArmyConvert.ArmyToJson(village.ArmyOwned));
                    playerSummary.ArmiesTraveling.Add(armyTraveling);

                    playerSummary.ArmyAtHome += ArmyConvert.ArmyToJson(village.ArmyAtHome);
                    playerSummary.ArmyTraveling += armyTraveling;
                }

                //  Support data
                var playerSupport = villagesSupportByPlayerId.GetValueOrDefault(player.PlayerId);
                if (playerSupport != null)
                {
                    //  Support where the target is one of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.Any(v => v.VillageId == s.TargetVillageId)))
                        playerSummary.ArmySupportingSelf += ArmyConvert.ArmyToJson(support.SupportingArmy);

                    //  Support where the target isn't any of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.All(v => v.VillageId != s.TargetVillageId)))
                        playerSummary.ArmySupportingOthers += ArmyConvert.ArmyToJson(support.SupportingArmy);
                }

                jsonData.Add(playerSummary);
            }

            return Ok(jsonData);
        }

        [HttpGet("op-plan")]
        public async Task<IActionResult> GenerateOpPlan()
        {
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            //  TODO
            return NotFound();
        }
    }
}
