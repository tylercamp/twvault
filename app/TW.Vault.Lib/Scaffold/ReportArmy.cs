using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class ReportArmy : IScaffoldArmy
    {
        public ReportArmy()
        {
            ReportAttackerArmy = new HashSet<Report>();
            ReportAttackerLossesArmy = new HashSet<Report>();
            ReportDefenderArmy = new HashSet<Report>();
            ReportDefenderLossesArmy = new HashSet<Report>();
            ReportDefenderTravelingArmy = new HashSet<Report>();
        }

        public long ArmyId { get; set; }
        public int? Spear { get; set; }
        public int? Sword { get; set; }
        public int? Axe { get; set; }
        public int? Archer { get; set; }
        public int? Spy { get; set; }
        public int? Light { get; set; }
        public int? Marcher { get; set; }
        public int? Heavy { get; set; }
        public int? Ram { get; set; }
        public int? Catapult { get; set; }
        public int? Knight { get; set; }
        public int? Snob { get; set; }
        public int? Militia { get; set; }
        public short WorldId { get; set; }

        public World World { get; set; }
        public ICollection<Report> ReportAttackerArmy { get; set; }
        public ICollection<Report> ReportAttackerLossesArmy { get; set; }
        public ICollection<Report> ReportDefenderArmy { get; set; }
        public ICollection<Report> ReportDefenderLossesArmy { get; set; }
        public ICollection<Report> ReportDefenderTravelingArmy { get; set; }
    }
}
