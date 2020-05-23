
using System.Collections.Generic;

namespace TW.Vault.Scaffold.Seed
{
    public static class TranslationLanguageData
    {
        public static List<TranslationLanguage> Contents { get; } = new List<TranslationLanguage>
        {
            new TranslationLanguage { Id = 1, Name = "English" },
        };
    }
}