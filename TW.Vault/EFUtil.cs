using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold_Model;
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
    }
}
