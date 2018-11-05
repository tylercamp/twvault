using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class PlayerInfo
    {
        public String PlayerName { get; set; }
        public long PlayerId { get; set; }
        public String TribeName { get; set; }
        public String TribeTag { get; set; }
        public long? TribeId { get; set; }
    }
}
