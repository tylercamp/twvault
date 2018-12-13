using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class PhoneNumber
    {
        public String Number { get; set; }
        public int Id { get; set; }
        public String Label { get; set; }
    }
}
