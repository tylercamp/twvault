using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Scaffold
{
    public partial class TranslationKey
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public bool IsTwNative { get; set; }
        public String Group { get; set; }
        public String Note { get; set; }

        public ICollection<TranslationEntry> TranslationEntries { get; set; }
        public ICollection<TranslationParameter> Parameters { get; set; }
    }
}
