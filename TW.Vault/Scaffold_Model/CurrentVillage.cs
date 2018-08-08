using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class CurrentVillage
    {
        public long VillageId { get; set; }
        public string VillageName { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public long? PlayerId { get; set; }
        public short? Points { get; set; }
        public long? ArmyStationedId { get; set; }
        public long? ArmyTravelingId { get; set; }
        public long? ArmyOwnedId { get; set; }

        public CurrentArmy ArmyOwned { get; set; }
        public CurrentArmy ArmyStationed { get; set; }
        public CurrentArmy ArmyTraveling { get; set; }
        public CurrentBuilding CurrentBuilding { get; set; }
    }
}
