using System.Collections.Generic;

namespace TW.Vault.Scaffold.Seed
{
    public static class TranslationRegistryData
    {
        public static List<TranslationRegistry> Contents { get; } = new List<TranslationRegistry>
        {
            new TranslationRegistry { Id = 1, Name = "Default", Author = "tcamps", AuthorPlayerId = 11301059, LanguageId = 1, IsSystemInternal = false }
        };
    }
}