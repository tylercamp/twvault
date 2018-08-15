using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class VillageArmySet
    {
        [Required]
        public long? VillageId { get; set; }

        [Required]
        public Army Stationed { get; set; }
        [Required]
        public Army Traveling { get; set; }
        [Required]
        public Army Supporting { get; set; }
        [Required]
        public Army AtHome { get; set; }

        public bool IsEmpty => Stationed != null || Traveling != null || Supporting != null || AtHome != null;
    }
}
