using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Report
    {
        public int ReportId { get; set; }
        public int AttackerVillageId { get; set; }
        public int DefenderVillageId { get; set; }
        public int AttackerPlayerId { get; set; }
        public int DefenderPlayerId { get; set; }
        public DateTime OccuredAt { get; set; }
        public int AttackerArmyId { get; set; }
        public int AttackerLossesArmyId { get; set; }
        public int? DefenderArmyId { get; set; }
        public int? DefenderLossesArmyId { get; set; }
        public int? DefenderTravelingArmyId { get; set; }

        public Army AttackerArmy { get; set; }
        public Army AttackerLossesArmy { get; set; }
        public Player AttackerPlayer { get; set; }
        public Village AttackerVillage { get; set; }
        public Army DefenderArmy { get; set; }
        public Army DefenderLossesArmy { get; set; }
        public Player DefenderPlayer { get; set; }
        public Army DefenderTravelingArmy { get; set; }
        public Village DefenderVillage { get; set; }
    }
}
