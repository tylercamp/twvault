using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class IncomingTag
    {
        public long CommandId { get; set; }
        public String OriginalTag { get; set; }
        public int NumFromVillage { get; set; }
        public int? OffensivePopulation { get; set; }
        public int? ReturningPopulation { get; set; }
        public int? NumCats { get; set; }
        public bool DefiniteFake { get; set; }
        public TroopType? TroopType { get; set; }
        public String VillageType { get; set; }
        public String SourceVillageName { get; set; }
        public String TargetVillageName { get; set; }
        public String SourcePlayerName { get; set; }
        public String SourceVillageCoords { get; set; }
        public String TargetVillageCoords { get; set; }
        public float Distance { get; set; }
    }
}
