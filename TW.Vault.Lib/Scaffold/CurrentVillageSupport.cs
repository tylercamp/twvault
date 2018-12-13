using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class CurrentVillageSupport
    {
        public long SourceVillageId { get; set; }
        public long TargetVillageId { get; set; }
        public short WorldId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public long SupportingArmyId { get; set; }
        public long Id { get; set; }
        public long? TxId { get; set; }
        public int AccessGroupId { get; set; }

        public Village SourceVillage { get; set; }
        public CurrentArmy SupportingArmy { get; set; }
        public Village TargetVillage { get; set; }
        public World World { get; set; }
        public Transaction Tx { get; set; }
    }
}
