using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class CurrentPlayer
    {
        public long PlayerId { get; set; }
        public short WorldId { get; set; }
        public int? CurrentPossibleNobles { get; set; }

        public int AccessGroupId { get; set; }

        public World World { get; set; }
    }
}
