using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class Conquer
    {
        public int VaultId { get; set; }
        public long VillageId { get; set; }
        public long UnixTimestamp { get; set; }
        public long? NewOwner { get; set; }
        public long? OldOwner { get; set; }
        public short WorldId { get; set; }

        public World World { get; set; }
    }
}
