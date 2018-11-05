using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Scaffold
{
    public partial class CustomInfo
    {
        public int Uid { get; set; }
        public long Id { get; set; }
        public String Data { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
