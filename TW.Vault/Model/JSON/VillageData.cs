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
        public Army AtHomeArmy { get; set; }

        public DateTime? StationedSeenAt { get; set; }
        public DateTime? TravelingSeenAt { get; set; }
        public DateTime? RecentlyLostArmySeenAt { get; set; }
        public DateTime? OwnedArmySeenAt { get; set; }
        public DateTime? AtHomeSeenAt { get; set; }

        public BuildingLevels LastBuildings { get; set; }
        public DateTime? LastBuildingsSeenAt { get; set; }

        public BuildingLevels PossibleBuildings { get; set; }

        public short? LastLoyalty { get; set; }
        public short? PossibleLoyalty { get; set; }
        public DateTime? LastLoyaltySeenAt { get; set; }

        public Army PossibleRecruitedDefensiveArmy { get; set; }
        public Army PossibleRecruitedOffensiveArmy { get; set; }

        public int? NukesRequired { get; set; }
        public int? LastNukeLossPercent { get; set; }

        public List<long> Fakes { get; set; }
        public List<long> Nobles { get; set; }
        public Dictionary<long, int> DVs { get; set; }
        public List<long> Nukes { get; set; }
        public List<long> Players { get; set; }

    }
}
