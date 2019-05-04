using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model
{
    //  Miscellaneous queries that I don't want to throw into a controller

    public static class UtilQuery
    {
        public static Task<Scaffold.Command> FindCommandForReport(Scaffold.Report report, Scaffold.VaultContext context)
        {
            return (
                from command in context.Command.FromWorld(report.WorldId).FromAccessGroup(report.AccessGroupId)
                where
                    command.TargetVillageId == report.DefenderVillageId &&
                    command.SourceVillageId == report.AttackerVillageId &&
                    command.LandsAt == report.OccuredAt &&
                    command.IsAttack

                select command
            ).FirstOrDefaultAsync();
        }
    }
}
