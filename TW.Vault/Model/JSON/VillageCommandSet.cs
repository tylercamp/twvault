using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class VillageCommandSet
    {
        public List<VillageCommand> CommandsToVillage { get; set; }
        public List<VillageCommand> CommandsFromVillage { get; set; }
    }
}
