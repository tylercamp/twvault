using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class ManyCommands
    {
        [Required]
        public bool? IsOwnCommands { get; set; }

        [Required]
        public List<Command> Commands { get; set; }
    }
}
