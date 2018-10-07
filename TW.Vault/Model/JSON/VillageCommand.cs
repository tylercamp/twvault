using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class VillageCommand
    {
        public long CommandId { get; set; }
        public DateTime LandsAt { get; set; }
        public DateTime ReturnsAt { get; set; }
        public bool IsReturning { get; set; }
        public String OtherVillageName { get; set; }
        public String OtherVillageCoords { get; set; }
        public long OtherVillageId { get; set; }
        public Army Army { get; set; }
        public String TroopType { get; set; }
    }
}
