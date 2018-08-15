using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Ally
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
