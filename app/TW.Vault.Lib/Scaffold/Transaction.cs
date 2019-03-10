using System;
using System.Collections.Generic;
using System.Net;

namespace TW.Vault.Scaffold
{
    public partial class Transaction
    {
        public Transaction()
        {
            Command = new HashSet<Command>();
            ConflictingDataRecordConflictingTx = new HashSet<ConflictingDataRecord>();
            ConflictingDataRecordOldTx = new HashSet<ConflictingDataRecord>();
            Report = new HashSet<Report>();
        }

        public long TxId { get; set; }
        public int Uid { get; set; }
        public DateTime OccurredAt { get; set; }
        public IPAddress Ip { get; set; }
        public short WorldId { get; set; }
        public long? PreviousTxId { get; set; }

        public World World { get; set; }
        public ICollection<Command> Command { get; set; }
        public ICollection<ConflictingDataRecord> ConflictingDataRecordConflictingTx { get; set; }
        public ICollection<ConflictingDataRecord> ConflictingDataRecordOldTx { get; set; }
        public ICollection<Report> Report { get; set; }
        public ICollection<CurrentVillageSupport> CurrentVillageSupport { get; set; }
        public ICollection<User> User { get; set; }
        public ICollection<UserLog> UserLog { get; set; }
        public ICollection<EnemyTribe> EnemyTribe { get; set; }
    }
}
