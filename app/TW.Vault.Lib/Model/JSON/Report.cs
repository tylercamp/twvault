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

        //  Attacker/defender may have deleted their accounts
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


        public static bool operator ==(Report a, Report b)
        {
            var aIsNull = ReferenceEquals(a, null);
            var bIsNull = ReferenceEquals(b, null);
            if (aIsNull != bIsNull)
                return false;

            if (aIsNull)
                return true;

            return
                a.ReportId == b.ReportId &&
                a.OccurredAt == b.OccurredAt &&
                a.Morale == b.Morale &&
                a.Luck == b.Luck &&
                a.Loyalty == b.Loyalty &&
                a.AttackingPlayerId == b.AttackingPlayerId &&
                a.DefendingPlayerId == b.DefendingPlayerId &&
                a.AttackingVillageId == b.AttackingVillageId &&
                a.DefendingVillageId == b.DefendingVillageId &&
                a.AttackingArmy == b.AttackingArmy &&
                a.AttackingArmyLosses == b.AttackingArmyLosses &&
                a.DefendingArmy == b.DefendingArmy &&
                a.DefendingArmyLosses == b.DefendingArmyLosses &&
                a.TravelingTroops == b.TravelingTroops &&
                a.DamagedBuildingLevels == b.DamagedBuildingLevels &&
                a.BuildingLevels == b.BuildingLevels;
        }

        public static bool operator !=(Report a, Report b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is Report)
                return this == (obj as Report);
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
