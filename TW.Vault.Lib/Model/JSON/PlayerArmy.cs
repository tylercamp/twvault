using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class PlayerArmy
    {
        public int? PossibleNobles { get; set; }
        public List<VillageArmySet> TroopData { get; set; }
    }
}
