using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class UserLog
    {
        public String AdminUserName { get; set; }
        public String EventDescription { get; set; }
        public DateTime? OccurredAt { get; set; }
    }
}
