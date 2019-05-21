using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class Village
    {
        public Village()
        {
            CurrentVillage = new HashSet<CurrentVillage>();
            CommandSourceVillage = new HashSet<Command>();
            CommandTargetVillage = new HashSet<Command>();
            CurrentVillageSupportSourceVillage = new HashSet<CurrentVillageSupport>();
            CurrentVillageSupportTargetVillage = new HashSet<CurrentVillageSupport>();
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
        public short WorldId { get; set; }

        public Player Player { get; set; }
        public World World { get; set; }
        public ICollection<CurrentVillage> CurrentVillage { get; set; }
        public ICollection<Command> CommandSourceVillage { get; set; }
        public ICollection<Command> CommandTargetVillage { get; set; }
        public ICollection<CurrentVillageSupport> CurrentVillageSupportSourceVillage { get; set; }
        public ICollection<CurrentVillageSupport> CurrentVillageSupportTargetVillage { get; set; }
        public ICollection<Report> ReportAttackerVillage { get; set; }
        public ICollection<Report> ReportDefenderVillage { get; set; }
    }
}
