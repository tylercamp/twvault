using System;
using System.Collections.Generic;

namespace TW.Vault.Lib.Scaffold
{
    public partial class Player
    {
        public Player()
        {
            CommandSourcePlayer = new HashSet<Command>();
            CommandTargetPlayer = new HashSet<Command>();
            ReportAttackerPlayer = new HashSet<Report>();
            ReportDefenderPlayer = new HashSet<Report>();
            Village = new HashSet<Village>();
        }

        public override int GetHashCode()
        {
            return
                PlayerId.GetHashCode() ^
                PlayerName.GetHashCode() ^
                (TribeId?.GetHashCode() ?? 0) ^
                (Villages?.GetHashCode() ?? 0) ^
                (Points?.GetHashCode() ?? 0) ^
                (PlayerRank?.GetHashCode() ?? 0) ^
                WorldId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null)) return false;
            return obj is Player && obj.GetHashCode() == this.GetHashCode();
        }

        public override string ToString()
        {
            return $"Player {PlayerName} (id={PlayerId})";
        }

        public long PlayerId { get; set; }
        public string PlayerName { get; set; }
        public long? TribeId { get; set; }
        public int? Villages { get; set; }
        public int? Points { get; set; }
        public int? PlayerRank { get; set; }
        public short WorldId { get; set; }

        public World World { get; set; }
        public Ally Tribe { get; set; }
        public ICollection<Command> CommandSourcePlayer { get; set; }
        public ICollection<Command> CommandTargetPlayer { get; set; }
        public ICollection<Report> ReportAttackerPlayer { get; set; }
        public ICollection<Report> ReportDefenderPlayer { get; set; }
        public ICollection<Village> Village { get; set; }
    }
}
