using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Simulation
{
    public class BattleResult
    {
        public Army AttackingArmy;
        public Army DefendingArmy;
        public int NewWallLevel;
    }
}
