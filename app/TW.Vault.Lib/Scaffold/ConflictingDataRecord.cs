using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class ConflictingDataRecord
    {
        public long Id { get; set; }
        public long ConflictingTxId { get; set; }
        public long OldTxId { get; set; }

        public Transaction ConflictingTx { get; set; }
        public Transaction OldTx { get; set; }
    }
}
