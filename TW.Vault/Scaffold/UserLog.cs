using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class UserLog
    {
        public long Id { get; set; }
        public long Uid { get; set; }
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
        public string OperationType { get; set; }

        public World World { get; set; }
    }
}
