using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class NotificationUserSettings
    {
        public int Uid { get; set; }
        public TimeSpan NotificationHeadroom { get; set; }
        public long TxId { get; set; }

        public Transaction Tx { get; set; }
        public User U { get; set; }
    }
}
