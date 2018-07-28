using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Player
    {
        public Player()
        {
            CommandSourcePlayer = new HashSet<Command>();
            CommandTargetPlayer = new HashSet<Command>();
            ReportAttackerPlayer = new HashSet<Report>();
            ReportDefenderPlayer = new HashSet<Report>();
        }

        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int? TribeId { get; set; }
        public int? Villages { get; set; }
        public int? Points { get; set; }
        public int? PlayerRank { get; set; }

        public ICollection<Command> CommandSourcePlayer { get; set; }
        public ICollection<Command> CommandTargetPlayer { get; set; }
        public ICollection<Report> ReportAttackerPlayer { get; set; }
        public ICollection<Report> ReportDefenderPlayer { get; set; }
    }
}
