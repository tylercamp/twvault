using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Notification
    {
        [Required]
        public String Message { get; set; }
        [Required]
        public DateTime? EventOccursAt { get; set; }
    }
}
