﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold;
using Microsoft.EntityFrameworkCore;

namespace TW.Vault
{
    public static class EFUtil
    {
        public static IQueryable<Report> IncludeReportData(this IQueryable<Report> reportsQuery) =>
            reportsQuery
                .Include(r => r.AttackerArmy)
                .Include(r => r.AttackerLossesArmy)
                .Include(r => r.DefenderArmy)
                .Include(r => r.DefenderLossesArmy)
                .Include(r => r.DefenderTravelingArmy)
                .Include(r => r.ReportBuilding);

        public static IQueryable<Command> IncludeCommandData(this IQueryable<Command> commandsQuery) =>
            commandsQuery
                .Include(c => c.Army);

        public static IQueryable<CurrentVillage> IncludeCurrentVillageData(this IQueryable<CurrentVillage> currentVillagesQuery) =>
            currentVillagesQuery
                .Include(v => v.ArmyOwned)
                .Include(v => v.ArmyRecentLosses)
                .Include(v => v.ArmyStationed)
                .Include(v => v.ArmyTraveling)
                .Include(v => v.CurrentBuilding);


        #region FromWorld

        public static IQueryable<Report> FromWorld(this IQueryable<Report> reportsQuery, long worldId) =>
            reportsQuery
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

        #endregion

    }
}
