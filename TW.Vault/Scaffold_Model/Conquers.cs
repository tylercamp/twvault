using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class Conquers
    {
        public int VaultId { get; set; }
        public int? VillageId { get; set; }
        public long? UnixTimestamp { get; set; }
        public int? NewOwner { get; set; }
        public int? OldOwner { get; set; }
    }
}
