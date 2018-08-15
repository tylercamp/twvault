using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Report
    {
        [Required]
        public long? ReportId { get; set; }
        [Required]
        public DateTime? OccurredAt { get; set; }
        [Required]
        public short? Morale { get; set; }
        [Required]
        public decimal? Luck { get; set; }

        public short? Loyalty { get; set; }

        [Required]
        public long? AttackingPlayerId { get; set; }

        public long? DefendingPlayerId { get; set; }

        [Required]
        public long? AttackingVillageId { get; set; }
        [Required]
        public long? DefendingVillageId { get; set; }

        [Required]
        public Army AttackingArmy { get; set; }
        [Required]
        public Army AttackingArmyLosses { get; set; }
        public Army DefendingArmy { get; set; }
        public Army DefendingArmyLosses { get; set; }
        public Army TravelingTroops { get; set; }

        public BuildingLevels DamagedBuildingLevels { get; set; }

        public BuildingLevels BuildingLevels { get; set; }
    }
}
