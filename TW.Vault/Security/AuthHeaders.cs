using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Security
{
    public class AuthHeaders
    {
        public Guid? AuthToken { get; set; }
        public long? TribeId { get; set; }
        public long? PlayerId { get; set; }
        public bool IsSitter { get; set; }

        public bool IsValid => AuthToken != null && TribeId != null && PlayerId != null;
    }
}
