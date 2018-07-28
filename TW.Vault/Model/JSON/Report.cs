using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Report
    {
        public int ReportId { get; set; }
        public DateTime OccurredAt { get; set; }

        public int AttackingPlayerId { get; set; }
        public int DefendingPlayerId { get; set; }

        public int AttackingVillageId { get; set; }
        public int DefendingVillageId { get; set; }

        public int[] AttackingArmy { get; set; }
        public int[] AttackingArmyLosses { get; set; }
        public int[] DefendingArmy { get; set; }
        public int[] DefendingArmyLosses { get; set; }
        public int[] TravelingTroops { get; set; }

        public Dictionary<String, int> BuildingLevels { get; set; }
    }
}
