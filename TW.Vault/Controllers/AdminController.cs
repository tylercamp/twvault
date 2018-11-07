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
using TW.Vault.Features.Simulation;
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

        [HttpGet("logs")]
        public async Task<IActionResult> GetUserLogs()
        {
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            var userLogs = await context.UserLog.FromWorld(CurrentWorldId).Include(l => l.Tx).OrderByDescending(l => l.TransactionTime).ToListAsync();
            var playerIds = userLogs
                .Select(l => l.PlayerId)
                .Concat(userLogs.Where(l => l.AdminPlayerId != null)
                .Select(l => l.AdminPlayerId.Value))
                .Distinct()
                .ToList();

            var playerNames = await (
                    from player in context.Player.FromWorld(CurrentWorldId)
                    where playerIds.Contains(player.PlayerId)
                    select new { player.PlayerId, player.PlayerName }
                ).ToListAsync();

            var userNames = await (
                    from user in context.User.FromWorld(CurrentWorldId)
                    join player in context.Player.FromWorld(CurrentWorldId) on user.PlayerId equals player.PlayerId
                    select new { user.Uid, player.PlayerName }
                ).ToListAsync();

            var playerNamesById = playerIds.ToDictionary(pid => pid, pid => WebUtility.UrlDecode(playerNames.SingleOrDefault(n => n.PlayerId == pid)?.PlayerName));
            var userNamesById = userNames.ToDictionary(u => u.Uid, u => WebUtility.UrlDecode(u.PlayerName));

            var logsByPlayerId = playerIds.ToDictionary(pid => pid, pid => userLogs.Where(l => l.PlayerId == pid).ToList());

            var result = new List<JSON.UserLog>();
            foreach (var log in userLogs)
            {
                var json = new JSON.UserLog();
                if (log.Tx != null)
                {
                    json.OccurredAt = log.Tx.OccurredAt;
                    json.AdminUserName = userNamesById[log.Tx.Uid];
                }
                else
                {
                    json.OccurredAt = log.TransactionTime;
                    json.AdminUserName = log.AdminPlayerId == null ? "System" : (playerNamesById[log.AdminPlayerId.Value] ?? "Unknown");
                }

                var logsForPlayer = logsByPlayerId[log.PlayerId].OrderBy(l => l.Tx?.OccurredAt ?? l.TransactionTime).ToList();
                int logIdx = logsForPlayer.IndexOf(log);
                var previousLog = logIdx > 0 ? logsForPlayer[logIdx - 1] : null;

                var playerName = playerNamesById[log.PlayerId] ?? "Unknown";

                switch (log.OperationType)
                {
                    case "INSERT":
                        json.EventDescription = $"Added key for {playerName}";
                        break;

                    case "UPDATE":
                        if (previousLog == null)
                        {
                            json.EventDescription = $"Updated {playerName} (unknown change)";
                        }
                        else
                        {
                            if (log.PermissionsLevel != previousLog.PermissionsLevel)
                            {
                                if (log.PermissionsLevel < (short)Security.PermissionLevel.Admin)
                                    json.EventDescription = $"Revoked admin priveleges for {playerName}";
                                else
                                    json.EventDescription = $"Gave admin priveleges to {playerName}";
                            }
                            else if (log.Enabled != previousLog.Enabled)
                            {
                                if (log.Enabled)
                                    json.EventDescription = $"Re-enabled key for {playerName}";
                                else
                                    json.EventDescription = $"Disabled key for {playerName}";
                            }
                            else
                            {
                                json.EventDescription = $"Updated {playerName} (unknown change)";
                            }
                        }
                        break;

                    case "DELETE":
                        json.EventDescription = $"Deleted key for {playerName}";
                        break;
                }

                result.Add(json);
            }

            return Ok(result.OrderByDescending(l => l.OccurredAt));
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
                    from user in context.User.FromWorld(CurrentWorldId)
                    join player in context.Player on user.PlayerId equals player.PlayerId
                    join tribe in context.Ally on player.TribeId equals tribe.TribeId into maybeTribe
                    from tribe in maybeTribe.DefaultIfEmpty()
                    where CurrentUser.KeySource == null || user.KeySource == CurrentUserId || !Configuration.Security.RestrictAccessWithinTribes
                    where user.Enabled && !user.IsReadOnly
                    where player.WorldId == CurrentWorldId
                    where (user.PermissionsLevel < (short)Security.PermissionLevel.System) || CurrentUserIsSystem
                    orderby tribe.Tag, player.PlayerName
                    select new { user, playerName = player.PlayerName, tribe = tribe }
                ).ToListAsync();

            if (Configuration.Security.RestrictAccessWithinTribes && !CurrentUserIsSystem)
                users = users.Where(u => u.tribe?.TribeId == CurrentTribeId || u.user.AdminAuthToken == CurrentAuthToken).ToList();

            var jsonUsers = users.Select(p => UserConvert.ModelToJson(
                p.user,
                WebUtility.UrlDecode(p.playerName),
                p.tribe != null ? WebUtility.UrlDecode(p.tribe.TribeName) : null
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
            newAuthUser.AdminAuthToken = CurrentAuthToken;
            newAuthUser.AdminPlayerId = CurrentPlayerId;
            newAuthUser.KeySource = CurrentUserId;
            newAuthUser.Label = player.PlayerName;
            newAuthUser.Tx = BuildTransaction();

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
                    where u.AuthToken == authKey && u.WorldId == CurrentWorldId
                    select u
                ).FirstOrDefaultAsync();

            if (requestedUser == null)
            {
                return BadRequest(new { error = "No user exists with that auth key." });
            }

            if (requestedUser.AuthToken == CurrentAuthToken)
            {
                return BadRequest(new { error = "You cannot delete your own key." });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System)
            {
                return BadRequest(new { error = "You cannot delete a system token." });
            }

            if (requestedUser.PermissionsLevel == (short)Security.PermissionLevel.Admin)
            {
                if (!CurrentUserIsSystem && requestedUser.KeySource.HasValue && requestedUser.KeySource.Value != CurrentUserId)
                {
                    return BadRequest(new { error = "You cannot delete an admin user that you have not created." });
                }
            }

            logger.LogWarning("User {SourceKey} disabling {TargetKey}", CurrentAuthToken, authKey);
            requestedUser.Enabled = false;
            requestedUser.AdminAuthToken = CurrentAuthToken;
            requestedUser.AdminPlayerId = CurrentPlayerId;
            requestedUser.TransactionTime = DateTime.UtcNow;
            requestedUser.Tx = BuildTransaction(requestedUser.Tx?.TxId);

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

            if (requestedUser.AuthToken == CurrentAuthToken)
            {
                return BadRequest(new { error = "You cannot change admin status of your own key." });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System && requestedUser.AdminAuthToken != CurrentUser.AuthToken)
            {
                return BadRequest(new { error = "You cannot change admin status of a user that you have not created." });
            }

            if (updateRequest.HasAdmin)
                requestedUser.PermissionsLevel = (short)Security.PermissionLevel.Admin;
            else
                requestedUser.PermissionsLevel = (short)Security.PermissionLevel.Default;

            requestedUser.TransactionTime = DateTime.UtcNow;
            requestedUser.Tx = BuildTransaction(requestedUser.Tx?.TxId);

            await context.SaveChangesAsync();
            return Ok();
        }


        [HttpGet("summary")]
        public async Task<IActionResult> GetTroopsSummary()
        {
            //  Dear jesus this is such a mess

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

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
                //  (This returns a lot of data and will be slow)
                from player in context.Player.FromWorld(CurrentWorldId)
                join user in context.User.FromWorld(CurrentWorldId) on player.PlayerId equals user.PlayerId
                join village in context.Village.FromWorld(CurrentWorldId) on player.PlayerId equals village.PlayerId
                join currentVillage in context.CurrentVillage.FromWorld(CurrentWorldId)
                                                             .Include(cv => cv.ArmyAtHome)
                                                             .Include(cv => cv.ArmyOwned)
                                                             .Include(cv => cv.ArmyTraveling)
                                    on village.VillageId equals currentVillage.VillageId
                where user.Enabled && !user.IsReadOnly
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select new { player, currentVillage }

                ,

                //  Get all CurrentPlayer data for the user's tribe (separate from global 'Player' table
                //      so we can also output stats for players that haven't uploaded anything yet)
                from currentPlayer in context.CurrentPlayer.FromWorld(CurrentWorldId)
                join player in context.Player.FromWorld(CurrentWorldId) on currentPlayer.PlayerId equals player.PlayerId
                join user in context.User.FromWorld(CurrentWorldId) on player.PlayerId equals user.PlayerId
                where user.Enabled && !user.IsReadOnly
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select currentPlayer
                
                ,

                //  Get user upload history
                from history in context.UserUploadHistory
                join user in context.User.FromWorld(CurrentWorldId) on history.Uid equals user.Uid
                join player in context.Player.FromWorld(CurrentWorldId) on user.PlayerId equals player.PlayerId
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                where user.Enabled && !user.IsReadOnly
                select new { playerId = player.PlayerId, history }
            );

            var villageIds = tribeVillages.Select(v => v.currentVillage.VillageId).Distinct().ToList();
            var attackedVillageIds = await Profile("Get incomings", () => (
                    from command in context.Command.FromWorld(CurrentWorldId)
                    where villageIds.Contains(command.TargetVillageId) && command.IsAttack && command.LandsAt > CurrentServerTime
                    select command.TargetVillageId
                ).ToListAsync());

            var attackingVillageIds = await Profile("Get attacks", () => (
                    from command in context.Command.FromWorld(CurrentWorldId)
                    where villageIds.Contains(command.SourceVillageId) && command.IsAttack && command.LandsAt > CurrentServerTime
                    select command.SourceVillageId
                ).ToListAsync());

            var tribeIds = tribeVillages.Select(tv => tv.player.TribeId)
                                        .Where(tid => tid != null)
                                        .Distinct()
                                        .Select(tid => tid.Value)
                                        .ToList();

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

            var villageIdsByPlayer = villagesByPlayer.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(v => v.VillageId).ToList()
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
            //  'tribeVillageIds' tends to be large, so this will be a slow query
            var villagesSupport = await (
                    from support in context.CurrentVillageSupport
                                           .FromWorld(CurrentWorldId)
                                           .Include(s => s.SupportingArmy)
                    where tribeVillageIds.Contains(support.SourceVillageId)
                    select support
                ).ToListAsync();
            


            //  Get support data by player Id, and sorted by target tribe ID
            var playersById = tribeVillages.Select(tv => tv.player).Distinct().ToDictionary(p => p.PlayerId, p => p);

            var tribeIdsByVillage = tribeVillages.ToDictionary(
                v => v.currentVillage.VillageId,
                v => v.player.TribeId ?? -1
            );

            //  Get tribes being supported that are not from vault
            var nonTribeVillageIds = villagesSupport.Select(s => s.TargetVillageId).Distinct().Except(tribeVillageIds).ToList();

            var nonTribeTargetTribesByVillageId = await (
                    from village in context.Village.FromWorld(CurrentWorldId)
                    join player in context.Player.FromWorld(CurrentWorldId) on village.PlayerId equals player.PlayerId
                    join ally in context.Ally.FromWorld(CurrentWorldId) on player.TribeId equals ally.TribeId
                    where nonTribeVillageIds.Contains(village.VillageId)
                    select new { village.VillageId, ally.TribeId }
                ).ToDictionaryAsync(d => d.VillageId, d => d.TribeId);

            foreach (var entry in nonTribeTargetTribesByVillageId)
                tribeIdsByVillage.Add(entry.Key, entry.Value);

            tribeIds = tribeIds.Concat(nonTribeTargetTribesByVillageId.Values.Distinct()).Distinct().ToList();

            var villagesSupportByPlayerId = new Dictionary<long, List<Scaffold.CurrentVillageSupport>>();
            var villagesSupportByPlayerIdByTargetTribeId = new Dictionary<long, Dictionary<long, List<Scaffold.CurrentVillageSupport>>>();


            foreach (var player in currentPlayers)
            {
                var supportFromPlayer = villagesSupport.Where(
                    s => villageIdsByPlayer[playersById[player.PlayerId]].Contains(s.SourceVillageId)
                ).ToList();

                villagesSupportByPlayerId.Add(player.PlayerId, supportFromPlayer);

                var supportByTribe = tribeIds.ToDictionary(tid => tid, _ => new List<Scaffold.CurrentVillageSupport>());
                supportByTribe.Add(-1, new List<Scaffold.CurrentVillageSupport>());

                foreach (var support in supportFromPlayer)
                {
                    var targetTribeId = tribeIdsByVillage.GetValueOrDefault(support.TargetVillageId, -1);
                    supportByTribe[targetTribeId].Add(support);
                }

                villagesSupportByPlayerIdByTargetTribeId.Add(player.PlayerId, supportByTribe);
            }

            var numIncomingsByPlayer = new Dictionary<long, int>();
            var numAttacksByPlayer = new Dictionary<long, int>();
            var villageOwnerIdById = tribeVillages.ToDictionary(v => v.currentVillage.VillageId, v => v.player.PlayerId);

            foreach (var target in attackedVillageIds)
            {
                var playerId = villageOwnerIdById[target];
                if (!numIncomingsByPlayer.ContainsKey(playerId))
                    numIncomingsByPlayer[playerId] = 0;
                numIncomingsByPlayer[playerId]++;
            }

            foreach (var source in attackingVillageIds)
            {
                var playerId = villageOwnerIdById[source];
                if (!numAttacksByPlayer.ContainsKey(playerId))
                    numAttacksByPlayer[playerId] = 0;
                numAttacksByPlayer[playerId]++;
            }
            
            var maxNoblesByPlayer = currentPlayers.ToDictionary(p => p.PlayerId, p => p.CurrentPossibleNobles);

            //  Get tribe labels
            var tribeNames = await (
                    from tribe in context.Ally.FromWorld(CurrentWorldId)
                    where tribeIds.Contains(tribe.TribeId)
                    select new { tribe.Tag, tribe.TribeId }
                ).ToListAsync();

            var tribeNamesById = tribeNames.ToDictionary(tn => tn.TribeId, tn => tn.Tag);

            var jsonData = new List<JSON.PlayerSummary>();
            foreach (var kvp in villagesByPlayer.OrderBy(kvp => kvp.Key.TribeId).ThenBy(kvp => kvp.Key.PlayerName))
            {
                var player = kvp.Key;
                String playerName = player.PlayerName;
                String tribeName = tribeNamesById.GetValueOrDefault(player.TribeId ?? -1);
                var playerVillages = kvp.Value;

                var playerHistory = uploadHistoryByPlayer.GetValueOrDefault(player.PlayerId);
                var playerSummary = new JSON.PlayerSummary
                {
                    PlayerName = WebUtility.UrlDecode(playerName),
                    PlayerId = player.PlayerId,
                    TribeName = tribeName,
                    UploadedAt = playerHistory?.LastUploadedTroopsAt ?? new DateTime(),
                    UploadedReportsAt = playerHistory?.LastUploadedReportsAt ?? new DateTime(),
                    UploadedIncomingsAt = playerHistory?.LastUploadedIncomingsAt ?? new DateTime(),
                    UploadedCommandsAt = playerHistory?.LastUploadedCommandsAt ?? new DateTime(),
                    NumNobles = playerVillages.Select(v => v.ArmyOwned?.Snob ?? 0).Sum(),
                    NumIncomings = numIncomingsByPlayer.GetValueOrDefault(player.PlayerId, 0),
                    NumAttackCommands = numAttacksByPlayer.GetValueOrDefault(player.PlayerId, 0)
                };

                playerSummary.UploadAge = DateTime.UtcNow - playerSummary.UploadedAt;

                if (maxNoblesByPlayer.ContainsKey(player.PlayerId))
                    playerSummary.MaxPossibleNobles = maxNoblesByPlayer[player.PlayerId];

                var oneNukePower = 500000.0f;
                var oneDvPower = 1850000.0f;

                //  General army data
                foreach (var village in playerVillages.Where(v => v.ArmyOwned != null && v.ArmyTraveling != null && v.ArmyAtHome != null))
                {
                    var armyOwned = ArmyConvert.ArmyToJson(village.ArmyOwned);
                    var armyTraveling = ArmyConvert.ArmyToJson(village.ArmyTraveling);
                    var armyAtHome = ArmyConvert.ArmyToJson(village.ArmyAtHome);

                    var armyPopSize = ArmyStats.CalculateTotalPopulation(armyOwned) / 20000.0f;
                    armyPopSize = Math.Clamp(armyPopSize, 0, 1);

                    bool isOffensive = village.ArmyOwned.Axe > 100;
                    if (isOffensive)
                    {
                        playerSummary.NumOffensiveVillages++;

                        var offensivePower = BattleSimulator.TotalAttackPower(armyOwned);

                        if (BattleSimulator.TotalAttackPower(armyOwned) >= oneNukePower)
                            playerSummary.NukesOwned++;
                        if (BattleSimulator.TotalAttackPower(armyTraveling) >= oneNukePower)
                            playerSummary.NukesTraveling++;
                    }
                    else
                    {
                        playerSummary.NumDefensiveVillages++;

                        var ownedDefensivePower = BattleSimulator.TotalDefensePower(armyOwned);
                        var atHomeDefensivePower = BattleSimulator.TotalDefensePower(armyAtHome);
                        var travelingDefensivePower = BattleSimulator.TotalDefensePower(armyTraveling);

                        playerSummary.DVsAtHome += atHomeDefensivePower / oneDvPower;
                        playerSummary.DVsOwned += ownedDefensivePower / oneDvPower;
                        playerSummary.DVsTraveling += travelingDefensivePower / oneDvPower;
                    }
                }

                //  Support data
                var playerSupport = villagesSupportByPlayerId.GetValueOrDefault(player.PlayerId);
                if (playerSupport != null)
                {
                    //  Support where the target is one of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.Any(v => v.VillageId == s.TargetVillageId)))
                        playerSummary.DVsSupportingSelf += BattleSimulator.TotalDefensePower(ArmyConvert.ArmyToJson(support.SupportingArmy)) / oneDvPower;

                    //  Support where the target isn't any of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.All(v => v.VillageId != s.TargetVillageId)))
                        playerSummary.DVsSupportingOthers += BattleSimulator.TotalDefensePower(ArmyConvert.ArmyToJson(support.SupportingArmy)) / oneDvPower;

                    playerSummary.SupportPopulationByTargetTribe = new Dictionary<string, int>();

                    foreach (var (tribeId, supportToTribe) in villagesSupportByPlayerIdByTargetTribeId[player.PlayerId])
                    {
                        var supportedTribeName = tribeNamesById.GetValueOrDefault(tribeId, "Unknown");
                        var totalSupportPopulation = 0;
                        foreach (var support in supportToTribe)
                            totalSupportPopulation += ArmyStats.CalculateTotalPopulation(ArmyConvert.ArmyToJson(support.SupportingArmy));

                        playerSummary.SupportPopulationByTargetTribe.Add(supportedTribeName, totalSupportPopulation);
                    }
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
