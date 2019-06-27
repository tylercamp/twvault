using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Features.Simulation;
using TW.Vault.Model.Convert;
using TW.Vault.Model.Native;
using TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Admin")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class AdminController : BaseController
    {
        public AdminController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
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

            var userLogs = await CurrentSets.UserLog.OrderByDescending(l => l.Id).Include(l => l.Tx).ToListAsync();
            var playerIds = userLogs
                .Select(l => l.PlayerId)
                .Concat(userLogs.Where(l => l.AdminPlayerId != null)
                .Select(l => l.AdminPlayerId.Value))
                .Distinct()
                .ToList();

            var playerNames = await (
                    from player in CurrentSets.Player
                    where playerIds.Contains(player.PlayerId)
                    select new { player.PlayerId, player.PlayerName }
                ).ToListAsync();

            var userNames = await (
                    from user in CurrentSets.User
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    select new { user.Uid, player.PlayerName }
                ).ToListAsync();

            var playerNamesById = playerIds.ToDictionary(pid => pid, pid => playerNames.SingleOrDefault(n => n.PlayerId == pid)?.PlayerName?.UrlDecode());
            var userNamesById = userNames.ToDictionary(u => u.Uid, u => u.PlayerName.UrlDecode());

            var logsByKey = userLogs.Select(l => l.AuthToken).Distinct().ToDictionary(t => t, t => userLogs.Where(l => l.AuthToken == t).ToList());

            var result = new List<JSON.UserLog>();
            foreach (var log in userLogs)
            {
                var json = new JSON.UserLog();
                if (log.Tx != null)
                {
                    json.OccurredAt = log.Tx.OccurredAt;
                    json.AdminUserName = log.Tx.Uid == 0 ? "System" : userNamesById[log.Tx.Uid];
                }
                else
                {
                    json.OccurredAt = log.TransactionTime;
                    json.AdminUserName = log.AdminPlayerId == null ? "System" : (playerNamesById.GetValueOrDefault(log.AdminPlayerId.Value) ?? Translate("UNKNOWN"));
                }

                var logsForKey = logsByKey[log.AuthToken].OrderBy(l => l.Id).ToList();
                int logIdx = logsForKey.IndexOf(log);
                var previousLog = logIdx > 0 ? logsForKey[logIdx - 1] : null;

                var playerName = playerNamesById[log.PlayerId] ?? Translate("UNKNOWN");

                switch (log.OperationType)
                {
                    case "INSERT":
                        json.EventDescription = Translate("ADMIN_LOG_ADDED_KEY_FOR", new { playerName });
                        break;

                    case "UPDATE":
                        var description = new List<String>();

                        if (previousLog != null)
                        {
                            if (log.PermissionsLevel != previousLog.PermissionsLevel)
                            {
                                if (log.PermissionsLevel < (short)Security.PermissionLevel.Admin)
                                    description.Add(Translate("ADMIN_LOG_REVOKED_PRIVELEGES_FOR", new { playerName }));
                                else
                                    description.Add(Translate("ADMIN_LOG_GAVE_PRIVELEGES_TO", new { playerName }));
                            }

                            if (log.Enabled != previousLog.Enabled)
                            {
                                if (log.Enabled)
                                    description.Add(Translate("ADMIN_LOG_RE_ENABLED_KEY_FOR", new { playerName }));
                                else
                                    description.Add(Translate("ADMIN_LOG_DISABLED_KEY_FOR", new { playerName }));
                            }

                            if (log.PlayerId != previousLog.PlayerId)
                            {
                                description.Add(Translate("ADMIN_LOG_CHANGED_KEY_OWNER", new {
                                    oldOwner = playerNamesById[previousLog.PlayerId],
                                    newOwner = playerNamesById[log.PlayerId]
                                }));
                            }

                            if (log.IsReadOnly != previousLog.IsReadOnly)
                            {
                                if (log.IsReadOnly)
                                    description.Add(Translate("ADMIN_LOG_SET_READ_ONLY", new { playerName = playerNamesById[log.PlayerId] }));
                                else
                                    description.Add(Translate("ADMIN_LOG_REMOVED_READ_ONLY", new { playerName = playerNamesById[log.PlayerId] }));
                            }

                            if (log.WorldId != previousLog.WorldId)
                            {
                                description.Add(Translate("ADMIN_LOG_CHANGED_SERVER", new { playerName = playerNamesById[log.PlayerId] }));
                            }

                            if (log.AdminAuthToken != previousLog.AdminAuthToken)
                            {
                                if (previousLog.AdminAuthToken != null && log.AdminAuthToken == null)
                                    description.Add(Translate("ADMIN_LOG_CLEARED_ADMIN", new {
                                        playerName = playerNamesById[previousLog.PlayerId]
                                    }));
                                else if (previousLog.AdminAuthToken == null && log.AdminAuthToken != null)
                                    description.Add(Translate("ADMIN_LOG_ASSIGNED_ADMIN", new {
                                        adminName = playerNamesById[log.AdminPlayerId.Value],
                                        playerName = playerNamesById[log.PlayerId]
                                    }));
                                else if (description.Count == 0)
                                    description.Add(Translate("ADMIN_LOG_CHANGED_ADMIN", new {
                                        playerName = playerNamesById[log.PlayerId],
                                        oldAdmin = playerNamesById[previousLog.AdminPlayerId.Value],
                                        newAdmin = playerNamesById[log.AdminPlayerId.Value]
                                    }));
                            }
                        }

                        if (description.Count == 0)
                            json.EventDescription = Translate("ADMIN_LOG_UNKNOWN_CHANGE", new { playerName });
                        else
                            json.EventDescription = String.Join("; ", description);

                        break;

                    case "DELETE":
                        json.EventDescription = Translate("ADMIN_LOG_DELETED_KEY", new { playerName });
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
                    from user in CurrentSets.ActiveUser
                    join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                    join tribe in CurrentSets.Ally on player.TribeId equals tribe.TribeId into maybeTribe
                    from tribe in maybeTribe.DefaultIfEmpty()
                    where CurrentUser.KeySource == null || user.KeySource == CurrentUserId || !Configuration.Security.RestrictAccessWithinTribes
                    where (user.PermissionsLevel < (short)Security.PermissionLevel.System) || CurrentUserIsSystem
                    orderby tribe.Tag, player.PlayerName
                    select new { user, playerName = player.PlayerName, tribe = tribe }
                ).ToListAsync();

            if (Configuration.Security.RestrictAccessWithinTribes && !CurrentUserIsSystem)
                users = users.Where(u => u.tribe?.TribeId == CurrentTribeId || u.user.AdminAuthToken == CurrentAuthToken).ToList();

            var jsonUsers = users.Select(p => UserConvert.ModelToJson(
                p.user,
                p.playerName.UrlDecode(),
                p.tribe != null ? p.tribe.TribeName.UrlDecode() : null
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
                        from p in CurrentSets.Player
                        where p.PlayerId == playerId
                        select p
                    ).FirstOrDefaultAsync();

                if (possiblePlayer == null)
                {
                    return BadRequest(new { error = Translate("ADMIN_PLAYER_NOT_FOUND_ID") });
                }

                player = possiblePlayer;
            }
            else if (keyRequest.PlayerName != null)
            {
                var formattedPlayerName = keyRequest.PlayerName.UrlEncode();

                var possiblePlayer = await (
                        from p in CurrentSets.Player
                        where p.PlayerName == formattedPlayerName
                        select p
                    ).FirstOrDefaultAsync();

                if (possiblePlayer == null)
                {
                    return BadRequest(new { error = Translate("ADMIN_PLAYER_NOT_FOUND_NAME") });
                }

                player = possiblePlayer;
            }
            else
            {
                return BadRequest(new { error = Translate("ADMIN_PLAYER_NAME_NOT_SET") });
            }

            if (!CurrentUserIsSystem && player.TribeId != CurrentTribeId && Configuration.Security.RestrictAccessWithinTribes)
            {
                return BadRequest(new { error = Translate("ADMIN_PLAYER_NOT_IN_TRIBE") });
            }

            bool userExists = await (
                    from user in CurrentSets.User
                    where user.PlayerId == player.PlayerId
                    where user.WorldId == null || user.WorldId == CurrentWorldId
                    where user.Enabled
                    select user
                ).AnyAsync();

            if (userExists)
            {
                return BadRequest(new { error = Translate("ADMIN_PLAYER_HAS_KEY") });
            }

            var newAuthUser = new Scaffold.User();
            newAuthUser.WorldId = CurrentWorldId;
            newAuthUser.PlayerId = player.PlayerId;
            newAuthUser.AccessGroupId = CurrentAccessGroupId;
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
            jsonUser.PlayerName = player.PlayerName.UrlDecode();

            var playerTribe = await (
                    from tribe in CurrentSets.Ally
                    where tribe.TribeId == player.TribeId
                    select tribe
                ).FirstOrDefaultAsync();

            jsonUser.TribeName = playerTribe?.TribeName?.UrlDecode();

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
                return BadRequest(new { error = Translate("ADMIN_INVALID_KEY") });
            }

            var requestedUser = await (
                    from u in context.User
                    where u.AuthToken == authKey && u.WorldId == CurrentWorldId
                    select u
                ).FirstOrDefaultAsync();

            if (requestedUser == null)
            {
                return BadRequest(new { error = Translate("ADMIN_KEY_NOT_FOUND") });
            }

            if (requestedUser.AuthToken == CurrentAuthToken)
            {
                return BadRequest(new { error = Translate("ADMIN_DELETE_OWN_KEY") });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System)
            {
                return BadRequest(new { error = Translate("ADMIN_DELETE_SYSTEM_KEY") });
            }

            if (requestedUser.PermissionsLevel == (short)Security.PermissionLevel.Admin)
            {
                if (!CurrentUserIsSystem && requestedUser.KeySource.HasValue && requestedUser.KeySource.Value != CurrentUserId)
                {
                    return BadRequest(new { error = Translate("ADMIN_DELETE_OTHER_ADMIN") });
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
                return BadRequest(new { error = Translate("ADMIN_INVALID_KEY") });
            }

            var requestedUser = await (
                    from u in context.User
                    where u.AuthToken == authKey
                    select u
                ).FirstOrDefaultAsync();

            if (requestedUser == null)
            {
                return BadRequest(new { error = Translate("ADMIN_KEY_NOT_FOUND") });
            }

            if (requestedUser.AuthToken == CurrentAuthToken)
            {
                return BadRequest(new { error = Translate("ADMIN_CHANGE_OWN_KEY") });
            }

            if (requestedUser.PermissionsLevel >= (short)Security.PermissionLevel.System && requestedUser.AdminAuthToken != CurrentUser.AuthToken)
            {
                return BadRequest(new { error = Translate("ADMIN_CHANGE_OTHER_ADMIN") });
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
        public async Task<IActionResult> GetTroopsSummary(int fangMinCats, int fangMaxPop)
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

            var (tribeVillages, currentPlayers, uploadHistory, enemyVillages) = await ManyTasks.RunToList(
                //  Get all CurrentVillages from the user's tribe - list of (Player, CurrentVillage)
                //  (This returns a lot of data and will be slow)
                from player in CurrentSets.Player
                join user in CurrentSets.User on player.PlayerId equals user.PlayerId
                join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                join currentVillage in CurrentSets.CurrentVillage
                                    on village.VillageId equals currentVillage.VillageId
                    into currentVillage
                where user.Enabled && !user.IsReadOnly
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select new { player, villageId = village.VillageId, currentVillage = currentVillage.FirstOrDefault(), X = village.X.Value, Y = village.Y.Value }

                ,

                //  Get all CurrentPlayer data for the user's tribe (separate from global 'Player' table
                //      so we can also output stats for players that haven't uploaded anything yet)
                from currentPlayer in CurrentSets.CurrentPlayer
                join player in CurrentSets.Player on currentPlayer.PlayerId equals player.PlayerId
                join user in CurrentSets.User on player.PlayerId equals user.PlayerId
                where user.Enabled && !user.IsReadOnly
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                select currentPlayer
                
                ,

                //  Get user upload history
                from history in context.UserUploadHistory
                join user in CurrentSets.User on history.Uid equals user.Uid
                join player in CurrentSets.Player on user.PlayerId equals player.PlayerId
                where player.TribeId == CurrentTribeId || !Configuration.Security.RestrictAccessWithinTribes
                where user.Enabled && !user.IsReadOnly
                select new { playerId = player.PlayerId, history }

                ,

                //  Get enemy villages
                from tribe in CurrentSets.EnemyTribe
                join player in CurrentSets.Player on tribe.EnemyTribeId equals player.TribeId
                join village in CurrentSets.Village on player.PlayerId equals village.PlayerId
                select new { village.VillageId, X = village.X.Value, Y = village.Y.Value }

            );

            // Need to load armies separately since `join into` doesn't work right with .Include()
            var armyIds = tribeVillages
                .Where(v => v.currentVillage != null)
                .SelectMany(v => new[] {
                    v.currentVillage.ArmyAtHomeId,
                    v.currentVillage.ArmyOwnedId,
                    v.currentVillage.ArmyRecentLossesId,
                    v.currentVillage.ArmyStationedId,
                    v.currentVillage.ArmySupportingId,
                    v.currentVillage.ArmyTravelingId
                })
                .Where(id => id != null)
                .Select(id => id.Value)
                .ToList();

            var allArmies = await context.CurrentArmy.Where(a => armyIds.Contains(a.ArmyId)).ToDictionaryAsync(a => a.ArmyId, a => a);
            foreach (var village in tribeVillages.Where(v => v.currentVillage != null))
            {
                Scaffold.CurrentArmy FindArmy(long? armyId) => armyId == null ? null : allArmies.GetValueOrDefault(armyId.Value);

                var cv = village.currentVillage;
                cv.ArmyAtHome = FindArmy(cv.ArmyAtHomeId);
                cv.ArmyOwned = FindArmy(cv.ArmyOwnedId);
                cv.ArmyRecentLosses = FindArmy(cv.ArmyRecentLossesId);
                cv.ArmyStationed = FindArmy(cv.ArmyStationedId);
                cv.ArmySupporting = FindArmy(cv.ArmySupportingId);
                cv.ArmyTraveling = FindArmy(cv.ArmyTravelingId);
            }

            var currentPlayerIds = currentPlayers.Select(p => p.PlayerId).ToList();

            var villageIds = tribeVillages.Select(v => v.villageId).Distinct().ToList();
            var attackedVillageIds = await Profile("Get incomings", () => (
                    from command in CurrentSets.Command
                    where villageIds.Contains(command.TargetVillageId) && command.IsAttack && command.LandsAt > CurrentServerTime && !command.IsReturning
                    where !currentPlayerIds.Contains(command.SourcePlayerId)
                    select command.TargetVillageId
                ).ToListAsync());

            var attackCommands = await Profile("Get attack details", () => (
                    from command in CurrentSets.Command.Include(c => c.Army)
                    where villageIds.Contains(command.SourceVillageId) && command.IsAttack && command.LandsAt > CurrentServerTime
                    where command.TargetPlayerId != null
                    select new { command.SourceVillageId, command.Army }
                ).ToListAsync());

            var attackingVillageIds = attackCommands.Select(c => c.SourceVillageId).ToList();

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
                                                              .Where(cv => cv != null)
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
            //  'villageIds' tends to be large, so this will be a slow query
            var villagesSupport = await (
                    from support in CurrentSets.CurrentVillageSupport
                                           .Include(s => s.SupportingArmy)
                    where villageIds.Contains(support.SourceVillageId)
                    select support
                ).ToListAsync();
            


            //  Get support data by player Id, and sorted by target tribe ID
            var playersById = tribeVillages.Select(tv => tv.player).Distinct().ToDictionary(p => p.PlayerId, p => p);

            var tribeIdsByVillage = tribeVillages.ToDictionary(
                v => v.villageId,
                v => v.player.TribeId ?? -1
            );

            //  Get tribes being supported that are not from vault
            var nonTribeVillageIds = villagesSupport.Select(s => s.TargetVillageId).Distinct().Except(villageIds).ToList();

            var nonTribeTargetTribesByVillageId = await (
                    from village in CurrentSets.Village
                    join player in CurrentSets.Player on village.PlayerId equals player.PlayerId
                    join ally in CurrentSets.Ally on player.TribeId equals ally.TribeId
                    where nonTribeVillageIds.Contains(village.VillageId)
                    select new { village.VillageId, ally.TribeId }
                ).ToDictionaryAsync(d => d.VillageId, d => d.TribeId);

            foreach (var entry in nonTribeTargetTribesByVillageId)
                tribeIdsByVillage.Add(entry.Key, entry.Value);

            tribeIds = tribeIds.Concat(nonTribeTargetTribesByVillageId.Values.Distinct()).Distinct().ToList();

            var villagesSupportByPlayerId = new Dictionary<long, List<Scaffold.CurrentVillageSupport>>();
            var villagesSupportByPlayerIdByTargetTribeId = new Dictionary<long, Dictionary<long, List<Scaffold.CurrentVillageSupport>>>();


            //  Only check support with players that have registered villas
            foreach (var player in currentPlayers.Where(p => playersById.ContainsKey(p.PlayerId)))
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
            var numAttackingFangsByPlayer = new Dictionary<long, int>();
            var villageOwnerIdById = tribeVillages.ToDictionary(v => v.villageId, v => v.player.PlayerId);

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

            bool IsFang(JSON.Army army, bool ignorePop = false) =>
                    army != null &&
                    army.ContainsKey(JSON.TroopType.Catapult) &&
                    army[JSON.TroopType.Catapult] >= fangMinCats &&
                    (ignorePop || ArmyStats.CalculateTotalPopulation(army, ArmyStats.OffensiveTroopTypes) <= fangMaxPop);

            foreach (var command in attackCommands)
            {
                var playerId = villageOwnerIdById[command.SourceVillageId];
                if (!numAttackingFangsByPlayer.ContainsKey(playerId))
                    numAttackingFangsByPlayer[playerId] = 0;

                if (IsFang(command.Army))
                    numAttackingFangsByPlayer[playerId]++;
            }

            var villagesNearEnemy = new HashSet<long>();
            foreach (var village in tribeVillages)
            {
                var nearbyEnemyVillage = enemyVillages.FirstOrDefault(v =>
                {
                    var distance = Model.Coordinate.Distance(v.X, v.Y, village.X, village.Y);
                    return distance < 10;
                });

                if (nearbyEnemyVillage != null)
                    villagesNearEnemy.Add(village.villageId);
            }
            
            var maxNoblesByPlayer = currentPlayers.ToDictionary(p => p.PlayerId, p => p.CurrentPossibleNobles);

            //  Get tribe labels
            var tribeNames = await (
                    from tribe in CurrentSets.Ally
                    where tribeIds.Contains(tribe.TribeId)
                    select new { tribe.Tag, tribe.TribeId }
                ).ToListAsync();

            var tribeNamesById = tribeNames.ToDictionary(tn => tn.TribeId, tn => tn.Tag.UrlDecode());

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
                    PlayerName = playerName.UrlDecode(),
                    PlayerId = player.PlayerId,
                    TribeName = tribeName,
                    UploadedAt = playerHistory?.LastUploadedTroopsAt ?? new DateTime(),
                    UploadedReportsAt = playerHistory?.LastUploadedReportsAt ?? new DateTime(),
                    UploadedIncomingsAt = playerHistory?.LastUploadedIncomingsAt ?? new DateTime(),
                    UploadedCommandsAt = playerHistory?.LastUploadedCommandsAt ?? new DateTime(),
                    NumNobles = playerVillages.Select(v => v.ArmyOwned?.Snob ?? 0).Sum(),
                    NumIncomings = numIncomingsByPlayer.GetValueOrDefault(player.PlayerId, 0),
                    NumAttackCommands = numAttacksByPlayer.GetValueOrDefault(player.PlayerId, 0),
                    FangsTraveling = numAttackingFangsByPlayer.GetValueOrDefault(player.PlayerId, 0)
                };

                playerSummary.UploadAge = DateTime.UtcNow - playerSummary.UploadedAt;

                if (maxNoblesByPlayer.ContainsKey(player.PlayerId))
                    playerSummary.MaxPossibleNobles = maxNoblesByPlayer[player.PlayerId];

                //  General army data
                foreach (var village in playerVillages.Where(v => v.ArmyOwned != null && v.ArmyTraveling != null && v.ArmyAtHome != null))
                {
                    var armyOwned = ArmyConvert.ArmyToJson(village.ArmyOwned);
                    var armyTraveling = ArmyConvert.ArmyToJson(village.ArmyTraveling);
                    var armyAtHome = ArmyConvert.ArmyToJson(village.ArmyAtHome);

                    if (IsFang(armyAtHome, true) && !ArmyStats.IsNuke(armyOwned, 0.75))
                        playerSummary.FangsOwned++;

                    if (ArmyStats.IsOffensive(village.ArmyOwned))
                    {
                        playerSummary.NumOffensiveVillages++;

                        var offensivePower = BattleSimulator.TotalAttackPower(armyOwned);

                        if (ArmyStats.IsNuke(armyOwned))
                            playerSummary.NukesOwned++;
                        else if (ArmyStats.IsNuke(armyOwned, 0.75))
                            playerSummary.ThreeQuarterNukesOwned++;
                        else if (ArmyStats.IsNuke(armyOwned, 0.5))
                            playerSummary.HalfNukesOwned++;
                        else if (ArmyStats.IsNuke(armyOwned, 0.25))
                            playerSummary.QuarterNukesOwned++;

                        if (ArmyStats.IsNuke(armyTraveling))
                            playerSummary.NukesTraveling++;
                        else if (IsFang(armyTraveling))
                            playerSummary.FangsTraveling++;
                    }
                    else
                    {
                        playerSummary.NumDefensiveVillages++;

                        var ownedDefensivePower = BattleSimulator.TotalDefensePower(armyOwned);
                        var atHomeDefensivePower = BattleSimulator.TotalDefensePower(armyAtHome);
                        var travelingDefensivePower = BattleSimulator.TotalDefensePower(armyTraveling);

                        playerSummary.DVsAtHome += atHomeDefensivePower / (float)ArmyStats.FullDVDefensivePower;
                        if (!villagesNearEnemy.Contains(village.VillageId))
                            playerSummary.DVsAtHomeBackline += atHomeDefensivePower / (float)ArmyStats.FullDVDefensivePower;

                        playerSummary.DVsOwned += ownedDefensivePower / (float)ArmyStats.FullDVDefensivePower;
                        playerSummary.DVsTraveling += travelingDefensivePower / (float)ArmyStats.FullDVDefensivePower;
                    }
                }

                //  Support data
                var playerSupport = villagesSupportByPlayerId.GetValueOrDefault(player.PlayerId);
                if (playerSupport != null)
                {
                    //  Support where the target is one of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.Any(v => v.VillageId == s.TargetVillageId)))
                        playerSummary.DVsSupportingSelf += BattleSimulator.TotalDefensePower(support.SupportingArmy) / (float)ArmyStats.FullDVDefensivePower;

                    //  Support where the target isn't any of the players' own villages
                    foreach (var support in playerSupport.Where(s => playerVillages.All(v => v.VillageId != s.TargetVillageId)))
                        playerSummary.DVsSupportingOthers += BattleSimulator.TotalDefensePower(support.SupportingArmy) / (float)ArmyStats.FullDVDefensivePower;

                    playerSummary.SupportPopulationByTargetTribe = new Dictionary<string, int>();

                    foreach (var (tribeId, supportToTribe) in villagesSupportByPlayerIdByTargetTribeId[player.PlayerId])
                    {
                        var supportedTribeName = tribeNamesById.GetValueOrDefault(tribeId, Translate("UNKNOWN"));
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

        [HttpGet("enemies")]
        public async Task<IActionResult> GetEnemyTribes()
        {
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            var result = await (
                    from enemy in CurrentSets.EnemyTribe
                    join tribe in CurrentSets.Ally on enemy.EnemyTribeId equals tribe.TribeId
                    select new { tribe.TribeId, Tag = tribe.Tag.UrlDecode(), TribeName = tribe.TribeName.UrlDecode() }
                ).ToListAsync();

            return Ok(result);
        }

        [HttpPost("enemies/{nameOrTag}")]
        public async Task<IActionResult> AddEnemyTribe(String nameOrTag)
        {
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            nameOrTag = nameOrTag.UrlEncode();

            var discoveredTribe = await (
                    from tribe in CurrentSets.Ally
                    where tribe.Tag == nameOrTag || tribe.TribeName == nameOrTag
                    select tribe
                ).FirstOrDefaultAsync();

            if (discoveredTribe == null)
                return NotFound();

            var existingEnemy = await (
                    from enemy in CurrentSets.EnemyTribe
                    where enemy.EnemyTribeId == discoveredTribe.TribeId
                    select enemy
                ).FirstOrDefaultAsync();

            if (existingEnemy != null)
                return Conflict();

            var newEnemy = new Scaffold.EnemyTribe
            {
                EnemyTribeId = discoveredTribe.TribeId,
                WorldId = CurrentWorldId,
                AccessGroupId = CurrentAccessGroupId,
                Tx = BuildTransaction()
            };

            context.Add(newEnemy);

            await context.SaveChangesAsync();

            return Ok(new { discoveredTribe.TribeId, Tag = discoveredTribe.Tag.UrlDecode(), TribeName = discoveredTribe.TribeName.UrlDecode() });
        }

        [HttpDelete("enemies/{nameOrTag}")]
        public async Task<IActionResult> RemoveEnemyTribe(String nameOrTag)
        {
            if (!CurrentUserIsAdmin)
            {
                var authRecord = MakeFailedAuthRecord("User is not admin");
                context.Add(authRecord);
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            nameOrTag = nameOrTag.UrlEncode();

            var discoveredTribe = CurrentSets.Ally
                                         .Where(a => a.TribeName == nameOrTag || a.Tag == nameOrTag)
                                         .FirstOrDefault();

            if (discoveredTribe == null)
                return NotFound();

            var enemyEntry = CurrentSets.EnemyTribe
                                    .Where(e => e.EnemyTribeId == discoveredTribe.TribeId)
                                    .FirstOrDefault();

            if (enemyEntry == null)
                return NotFound();

            context.Remove(enemyEntry);
            context.SaveChanges();

            return Ok();
        }
    }
}
