using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class WorldSettings
    {
        public string Setting { get; set; }
        public string Value { get; set; }
        public short WorldId { get; set; }

        public World World { get; set; }
    }
}
