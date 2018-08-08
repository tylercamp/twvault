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

        public long VillageId { get; set; }
        public string VillageName { get; set; }
        public short? X { get; set; }
        public short? Y { get; set; }
        public long? PlayerId { get; set; }
        public short? Points { get; set; }
        public int? VillageRank { get; set; }

        public ICollection<Command> CommandSourceVillage { get; set; }
        public ICollection<Command> CommandTargetVillage { get; set; }
        public ICollection<Report> ReportAttackerVillage { get; set; }
        public ICollection<Report> ReportDefenderVillage { get; set; }
    }
}
