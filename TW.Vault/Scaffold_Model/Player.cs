using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Player
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int? TribeId { get; set; }
        public int? Villages { get; set; }
        public int? Points { get; set; }
        public int? PlayerRank { get; set; }
    }
}
