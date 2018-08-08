using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Ally
    {
        public long TribeId { get; set; }
        public string TribeName { get; set; }
        public string Tag { get; set; }
        public int? Members { get; set; }
        public int? Villages { get; set; }
        public long? Points { get; set; }
        public long? AllPoints { get; set; }
        public long? TribeRank { get; set; }
    }
}
