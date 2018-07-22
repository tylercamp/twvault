using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Village
    {
        public int VillageId { get; set; }
        public string VillageName { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? PlayerId { get; set; }
        public int? Points { get; set; }
        public int? VillageRank { get; set; }
    }
}
