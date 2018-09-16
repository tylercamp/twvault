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
            NotificationPhoneNumber = new HashSet<NotificationPhoneNumber>();
            NotificationRequest = new HashSet<NotificationRequest>();
            NotificationUserSettings = new HashSet<NotificationUserSettings>();
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
        public ICollection<NotificationPhoneNumber> NotificationPhoneNumber { get; set; }
        public ICollection<NotificationRequest> NotificationRequest { get; set; }
        public ICollection<NotificationUserSettings> NotificationUserSettings { get; set; }
        public ICollection<Report> Report { get; set; }
    }
}
