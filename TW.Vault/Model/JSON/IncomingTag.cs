using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class IncomingTag
    {
        public long CommandId { get; set; }
        public int NumFromVillage { get; set; }
        public int? OffensivePopulation { get; set; }
        public int? NumCats { get; set; }
        public bool DefiniteFake { get; set; }
        public TroopType? TroopType { get; set; }
    }
}
