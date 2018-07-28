using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Command
    {
        public int CommandId { get; set; }
        public int SourceVillageId { get; set; }
        public int SourcePlayerId { get; set; }
        public int TargetVillageId { get; set; }
        public int TargetPlayerId { get; set; }
        public string TroopType { get; set; }
        public DateTime LandsAt { get; set; }
        public DateTime FirstSeenAt { get; set; }

        public Player SourcePlayer { get; set; }
        public Village SourceVillage { get; set; }
        public Player TargetPlayer { get; set; }
        public Village TargetVillage { get; set; }
    }
}
