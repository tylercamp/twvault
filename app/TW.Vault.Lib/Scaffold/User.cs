using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class User
    {
        public User()
        {
            InvalidDataRecord = new HashSet<InvalidDataRecord>();
            UserUploadHistory = new HashSet<UserUploadHistory>();
        }

        public int Uid { get; set; }
        public long PlayerId { get; set; }
        public short PermissionsLevel { get; set; }
        public string Label { get; set; }
        public bool Enabled { get; set; }
        public Guid AuthToken { get; set; }
        public short? WorldId { get; set; }
        public int? KeySource { get; set; }
        public DateTime TransactionTime { get; set; }
        public Guid? AdminAuthToken { get; set; }
        public long? AdminPlayerId { get; set; }
        public bool IsReadOnly { get; set; }
        public long? TxId { get; set; }
        public int AccessGroupId { get; set; }


        public World World { get; set; }
        public Transaction Tx { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public ICollection<InvalidDataRecord> InvalidDataRecord { get; set; }
        public ICollection<UserUploadHistory> UserUploadHistory { get; set; }
    }
}
