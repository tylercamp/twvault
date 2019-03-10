using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Village
    {
        public long VillageId { get; set; }
        public string VillageName { get; set; }
        public short? X { get; set; }
        public short? Y { get; set; }
        public long? PlayerId { get; set; }
        public short? Points { get; set; }
        public int? VillageRank { get; set; }
    }
}
