using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Village
    {
        public Village()
        {
            CommandSourceVillage = new HashSet<Command>();
            CommandTargetVillage = new HashSet<Command>();
            ReportAttackerVillage = new HashSet<Report>();
            ReportDefenderVillage = new HashSet<Report>();
        }

        public int VillageId { get; set; }
        public string VillageName { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? PlayerId { get; set; }
        public int? Points { get; set; }
        public int? VillageRank { get; set; }

        public ICollection<Command> CommandSourceVillage { get; set; }
        public ICollection<Command> CommandTargetVillage { get; set; }
        public ICollection<Report> ReportAttackerVillage { get; set; }
        public ICollection<Report> ReportDefenderVillage { get; set; }
    }
}
