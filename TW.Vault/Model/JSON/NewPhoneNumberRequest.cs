using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class NewPhoneNumberRequest
    {
        [Required]
        public String PhoneNumber { get; set; }
        [Required]
        public String Label { get; set; }
    }
}
