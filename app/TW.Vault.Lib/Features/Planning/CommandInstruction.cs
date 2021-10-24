using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Model.JSON;

namespace TW.Vault.Lib.Features.Planning
{
    public class CommandInstruction
    {
        public long SendFrom { get; set; }
        public long SendTo { get; set; }

        public TimeSpan TravelTime { get; set; }
        public TroopType TroopType { get; set; }
    }
}
