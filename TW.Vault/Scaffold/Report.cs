using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class Report
    {
        public long ReportId { get; set; }
        public long AttackerVillageId { get; set; }
        public long DefenderVillageId { get; set; }
        public long? AttackerPlayerId { get; set; }
        public long? DefenderPlayerId { get; set; }
        public DateTime OccuredAt { get; set; }
        public long AttackerArmyId { get; set; }
        public long AttackerLossesArmyId { get; set; }
        public long? DefenderArmyId { get; set; }
        public long? DefenderLossesArmyId { get; set; }
        public long? DefenderTravelingArmyId { get; set; }
        public short Morale { get; set; }
        public decimal Luck { get; set; }
        public long? TxId { get; set; }
        public short WorldId { get; set; }
        public short? Loyalty { get; set; }
        public long? BuildingId { get; set; }
        public int AccessGroupId { get; set; }

        public ReportArmy AttackerArmy { get; set; }
        public ReportArmy AttackerLossesArmy { get; set; }
        public Player AttackerPlayer { get; set; }
        public Village AttackerVillage { get; set; }
        public ReportBuilding Building { get; set; }
        public ReportArmy DefenderArmy { get; set; }
        public ReportArmy DefenderLossesArmy { get; set; }
        public Player DefenderPlayer { get; set; }
        public ReportArmy DefenderTravelingArmy { get; set; }
        public Village DefenderVillage { get; set; }
        public Transaction Tx { get; set; }
        public World World { get; set; }
    }
}
