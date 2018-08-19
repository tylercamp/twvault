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
        public bool DefiniteFake { get; set; }
        public bool PossibleFang { get; set; }
    }
}
