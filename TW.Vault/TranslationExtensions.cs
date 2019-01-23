using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class TranslationExtensions
    {
        public static Dictionary<String, String> FillMissingKeys(this Dictionary<String, String> translation, Scaffold.VaultContext context, int fallbackTranslationId)
        {
            var missingKeys = context.TranslationKey.ToList().Where(k => !translation.ContainsKey(k.Name)).ToList();

            var result = new Dictionary<String, String>(translation);

            if (missingKeys.Count == 0)
                return result;

            var missingKeyIds = missingKeys.Select(k => k.Id).ToList();

            var fallbackTranslations = context.TranslationEntry
                .Where(e => e.TranslationId == fallbackTranslationId)
                .Where(e => missingKeyIds.Contains(e.KeyId))
                .ToList();

            foreach (var missingKey in missingKeys)
            {
                var entry = fallbackTranslations.FirstOrDefault(t => t.KeyId == missingKey.Id);
                if (entry == null)
                    continue;

                result[missingKey.Name] = entry.Value;
            }

            return result;
        }
    }
}
