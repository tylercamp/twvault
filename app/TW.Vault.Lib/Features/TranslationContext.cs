using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features
{
    public class TranslationContext
    {
        Scaffold.VaultContext context;
        List<short> fallbackIds;
        Dictionary<short, Scaffold.TranslationRegistry> loadedFallbacks;
        Dictionary<String, short> loadedKeyIds;
        Dictionary<short, String> loadedKeyNames;

        public TranslationContext(Scaffold.VaultContext context, Scaffold.TranslationRegistry currentRegistry, params short[] fallbackIds)
        {
            this.context = context;
            this.CurrentRegistry = currentRegistry;
            this.fallbackIds = fallbackIds.Distinct().ToList();

            this.loadedFallbacks = new Dictionary<short, Scaffold.TranslationRegistry>();
        }

        public Scaffold.TranslationRegistry CurrentRegistry { get; }

        public Scaffold.TranslationEntry this[short keyId]
        {
            get
            {
                foreach (var translation in AvailableRegistries)
                {
                    var entry = FindEntryInRegistry(translation, keyId);

                    if (entry != null)
                        return entry;
                }

                throw new KeyNotFoundException("Could not resolve translation key id=" + keyId);
            }
        }

        public Scaffold.TranslationEntry this[String keyName]
        {
            get => this[KeyIds[keyName]];
        }

        public Dictionary<String, String> GetFullTranslation()
        {
            var result = new Dictionary<String, String>();

            foreach (var registry in AvailableRegistries)
            {
                foreach (var entry in registry.Entries.Where(e => e.Value.Length > 0 && !result.ContainsKey(KeyNames[e.KeyId])))
                {
                    result.Add(KeyNames[entry.KeyId], entry.Value);
                }
            }

            return result;
        }

        public Dictionary<String, short> KeyIds
        {
            get
            {
                if (loadedKeyIds == null)
                    loadedKeyIds = context.TranslationKey.ToDictionary(k => k.Name, k => k.Id);

                return loadedKeyIds;
            }
        }

        public Dictionary<short, String> KeyNames
        {
            get
            {
                if (loadedKeyNames == null)
                    loadedKeyNames = KeyIds.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

                return loadedKeyNames;
            }
        }

        private IEnumerable<Scaffold.TranslationRegistry> AvailableRegistries
        {
            get
            {
                yield return CurrentRegistry;

                foreach (var fallbackId in fallbackIds)
                {
                    if (!loadedFallbacks.ContainsKey(fallbackId))
                        loadedFallbacks.Add(
                            fallbackId,
                            context.TranslationRegistry
                                   .Include(r => r.Entries)
                                   .Where(r => r.Id == fallbackId)
                                   .First()
                        );

                    var fallback = loadedFallbacks[fallbackId];
                    yield return fallback;
                }
            }
        }

        private Scaffold.TranslationEntry FindEntryInRegistry(Scaffold.TranslationRegistry registry, short keyId) =>
            registry.Entries.Where(e => e.KeyId == keyId).FirstOrDefault();
    }
}
