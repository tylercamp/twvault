using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Army
    {
        public Army()
        {
            ReportAttackerArmy = new HashSet<Report>();
            ReportAttackerLossesArmy = new HashSet<Report>();
            ReportDefenderArmy = new HashSet<Report>();
            ReportDefenderLossesArmy = new HashSet<Report>();
            ReportDefenderTravelingArmy = new HashSet<Report>();
        }

        public int ArmyId { get; set; }
        public int? UnitSpear { get; set; }
        public int? UnitSword { get; set; }
        public int? UnitAxe { get; set; }
        public int? UnitArcher { get; set; }
        public int? UnitSpy { get; set; }
        public int? UnitLight { get; set; }
        public int? UnitMarcher { get; set; }
        public int? UnitHeavy { get; set; }
        public int? UnitRam { get; set; }
        public int? UnitCatapult { get; set; }
        public int? UnitKnight { get; set; }
        public int? UnitSnob { get; set; }
        public int? UnitMilitia { get; set; }

        public ICollection<Report> ReportAttackerArmy { get; set; }
        public ICollection<Report> ReportAttackerLossesArmy { get; set; }
        public ICollection<Report> ReportDefenderArmy { get; set; }
        public ICollection<Report> ReportDefenderLossesArmy { get; set; }
        public ICollection<Report> ReportDefenderTravelingArmy { get; set; }
    }
}
