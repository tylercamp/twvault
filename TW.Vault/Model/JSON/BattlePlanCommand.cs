using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class BattlePlanCommand
    {
        public long SourceVillageId { get; set; }
        public long TargetVillageId { get; set; }

        public String SourceVillageName { get; set; }
        public String TargetVillageName { get; set; }

        public int SourceVillageX { get; set; }
        public int SourceVillageY { get; set; }
        public int TargetVillageX { get; set; }
        public int TargetVillageY { get; set; }

        public DateTime? LaunchAt { get; set; }
        public DateTime? LandsAt { get; set; }
        public TimeSpan TravelTime { get; set; }

        public String TroopType { get; set; }
    }
}
