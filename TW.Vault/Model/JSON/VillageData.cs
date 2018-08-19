using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class VillageData
    {
        public Army StationedArmy { get; set; }
        public Army TravelingArmy { get; set; }
        public Army RecentlyLostArmy { get; set; }
        public Army OwnedArmy { get; set; }

        public DateTime? StationedSeenAt { get; set; }
        public DateTime? TravelingSeenAt { get; set; }
        public DateTime? RecentlyLostArmySeenAt { get; set; }
        public DateTime? OwnedArmySeenAt { get; set; }

        public BuildingLevels LastBuildings { get; set; }
        public DateTime? LastBuildingsSeenAt { get; set; }

        public BuildingLevels PossibleBuildings { get; set; }

        public short? LastLoyalty { get; set; }
        public short? PossibleLoyalty { get; set; }
        public DateTime? LastLoyaltySeenAt { get; set; }

        public Army PossibleRecruitedDefensiveArmy { get; set; }
        public Army PossibleRecruitedOffensiveArmy { get; set; }

        public int? NukesRequired { get; set; }
    }
}
