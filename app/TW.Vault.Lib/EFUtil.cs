using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold;
using Microsoft.EntityFrameworkCore;

namespace TW.Vault
{
    public static class EFUtil
    {
        public static async Task<UserUploadHistory> GetOrCreateUserUploadHistory(VaultContext context, int uid)
        {
            var userUploadHistory = await context.UserUploadHistory.Where(h => h.Uid == uid).FirstOrDefaultAsync();
            if (userUploadHistory == null)
            {
                userUploadHistory = new UserUploadHistory();
                userUploadHistory.Uid = uid;
                context.Add(userUploadHistory);
            }
            return userUploadHistory;
        }

        public static async Task<CurrentPlayer> GetOrCreateCurrentPlayer(VaultContext context, long playerId, short worldId, int accessGroupId)
        {
            var currentPlayer = await context.CurrentPlayer.FromAccessGroup(accessGroupId).FromWorld(worldId).Where(p => p.PlayerId == playerId).FirstOrDefaultAsync();
            if (currentPlayer == null)
            {
                currentPlayer = new CurrentPlayer();
                currentPlayer.PlayerId = playerId;
                currentPlayer.WorldId = worldId;
                currentPlayer.AccessGroupId = accessGroupId;
                context.Add(currentPlayer);
            }
            return currentPlayer;
        }

        public static IQueryable<Report> IncludeReportData(this IQueryable<Report> reportsQuery) =>
            reportsQuery
                .Include(r => r.AttackerArmy)
                .Include(r => r.AttackerLossesArmy)
                .Include(r => r.DefenderArmy)
                .Include(r => r.DefenderLossesArmy)
                .Include(r => r.DefenderTravelingArmy)
                .Include(r => r.Building);

        public static IQueryable<Command> IncludeCommandData(this IQueryable<Command> commandsQuery) =>
            commandsQuery
                .Include(c => c.Army);

        public static IQueryable<CurrentVillage> IncludeCurrentVillageData(this IQueryable<CurrentVillage> currentVillagesQuery) =>
            currentVillagesQuery
                .Include(v => v.ArmyOwned)
                .Include(v => v.ArmyRecentLosses)
                .Include(v => v.ArmyStationed)
                .Include(v => v.ArmyTraveling)
                .Include(v => v.ArmyAtHome)
                .Include(v => v.ArmySupporting)
                .Include(v => v.CurrentBuilding);

        public static IQueryable<User> Active(this IQueryable<User> userQuery) =>
            userQuery.Where(u => u.Enabled && !u.IsReadOnly);

        #region FromWorld

        public static IQueryable<Report> FromWorld(this IQueryable<Report> reportsQuery, long worldId) =>
            reportsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<IgnoredReport> FromWorld(this IQueryable<IgnoredReport> ignoredReportsQuery, long worldId) =>
            ignoredReportsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<Command> FromWorld(this IQueryable<Command> commandsQuery, long worldId) =>
            commandsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<WorldSettings> FromWorld(this IQueryable<WorldSettings> worldSettingsQuery, long worldId) =>
            worldSettingsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<Ally> FromWorld(this IQueryable<Ally> alliesQuery, long worldId) =>
            alliesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CommandArmy> FromWorld(this IQueryable<CommandArmy> commandArmiesQuery, long worldId) =>
            commandArmiesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<Conquer> FromWorld(this IQueryable<Conquer> conquersQuery, long worldId) =>
            conquersQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CurrentArmy> FromWorld(this IQueryable<CurrentArmy> currentArmiesQuery, long worldId) =>
            currentArmiesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CurrentBuilding> FromWorld(this IQueryable<CurrentBuilding> currentBuildingsQuery, long worldId) =>
            currentBuildingsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CurrentVillage> FromWorld(this IQueryable<CurrentVillage> currentVillagesQuery, long worldId) =>
            currentVillagesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<Player> FromWorld(this IQueryable<Player> currentPlayersQuery, long worldId) =>
            currentPlayersQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<ReportArmy> FromWorld(this IQueryable<ReportArmy> currentReportArmiesQuery, long worldId) =>
            currentReportArmiesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<ReportBuilding> FromWorld(this IQueryable<ReportBuilding> currentReportBuildingsQuery, long worldId) =>
            currentReportBuildingsQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<Village> FromWorld(this IQueryable<Village> currentVillagesQuery, long worldId) =>
            currentVillagesQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CurrentPlayer> FromWorld(this IQueryable<CurrentPlayer> currentPlayerQuery, long worldId) =>
            currentPlayerQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<CurrentVillageSupport> FromWorld(this IQueryable<CurrentVillageSupport> currentVillageSupportQuery, long worldId) =>
            currentVillageSupportQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<User> FromWorld(this IQueryable<User> userQuery, long worldId) =>
            userQuery
                .Where(q => q.WorldId == null || q.WorldId == worldId);

        public static IQueryable<UserLog> FromWorld(this IQueryable<UserLog> userLogQuery, long worldId) =>
            userLogQuery
                .Where(q => q.WorldId == null || q.WorldId == worldId);

        public static IQueryable<Transaction> FromWorld(this IQueryable<Transaction> transactionQuery, long worldId) =>
            transactionQuery
                .Where(q => q.WorldId == worldId);

        public static IQueryable<EnemyTribe> FromWorld(this IQueryable<EnemyTribe> enemyTribeQuery, long worldId) =>
            enemyTribeQuery
                .Where(q => q.WorldId == worldId);

        #endregion

        #region FromAccessGroup

        public static IQueryable<User> FromAccessGroup(this IQueryable<User> userQuery, int accessGroupId) =>
            userQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<Command> FromAccessGroup(this IQueryable<Command> commandQuery, int accessGroupId) =>
            commandQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<CurrentBuilding> FromAccessGroup(this IQueryable<CurrentBuilding> buildingQuery, int accessGroupId) =>
            buildingQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<CurrentPlayer> FromAccessGroup(this IQueryable<CurrentPlayer> playerQuery, int accessGroupId) =>
            playerQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<CurrentVillage> FromAccessGroup(this IQueryable<CurrentVillage> villageQuery, int accessGroupId) =>
            villageQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<CurrentVillageSupport> FromAccessGroup(this IQueryable<CurrentVillageSupport> supportQuery, int accessGroupId) =>
            supportQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<EnemyTribe> FromAccessGroup(this IQueryable<EnemyTribe> enemyTribeQuery, int accessGroupId) =>
            enemyTribeQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<Report> FromAccessGroup(this IQueryable<Report> reportQuery, int accessGroupId) =>
            reportQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<IgnoredReport> FromAccessGroup(this IQueryable<IgnoredReport> ignoredReportsQuery, int accessGroupId) =>
            ignoredReportsQuery.Where(u => u.AccessGroupId == accessGroupId);

        public static IQueryable<UserLog> FromAccessGroup(this IQueryable<UserLog> userLogQuery, int accessGroupId) =>
            userLogQuery.Where(u => u.AccessGroupId == accessGroupId);

        #endregion

    }
}
