using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Scaffold
{
    public partial class TranslationRegistry
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public long AuthorPlayerId { get; set; }
        public short LanguageId { get; set; }
        public bool IsSystemInternal { get; set; }

        public TranslationLanguage Language { get; set; }

        public ICollection<TranslationEntry> Entries { get; set; }
        public ICollection<World> DefaultWorlds { get; set; }
    }
}
