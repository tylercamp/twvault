using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class BacktimeInfo
    {
        public int TravelingArmyPopulation { get; set; }
        public int ExistingBacktimes { get; set; }
        public bool IsStacked { get; set; }
        public String TargetPlayerName { get; set; }
        public String TargetTribeName { get; set; }
        public String TargetTribeTag { get; set; }
        public List<BattlePlanCommand> Instructions { get; set; }
    }
}
