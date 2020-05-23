using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Scaffold
{
    public partial class EnemyTribe
    {
        public int Id { get; set; }
        public short WorldId { get; set; }
        public long EnemyTribeId { get; set; }
        public long TxId { get; set; }
        public int AccessGroupId { get; set; }

        public AccessGroup AccessGroup { get; set; }
        public World World { get; set; }
        public Transaction Tx { get; set; }
    }
}
