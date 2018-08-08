using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class User
    {
        public int Uid { get; set; }
        public long PlayerId { get; set; }
        public short PermissionsLevel { get; set; }
        public string Label { get; set; }
        public bool Enabled { get; set; }
        public Guid AuthToken { get; set; }
    }
}
