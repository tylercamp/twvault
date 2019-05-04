using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class CachingMap<T> where T : class
    {
        ConcurrentDictionary<String, Cacher<T>> cachedContents = new ConcurrentDictionary<String, Cacher<T>>();

        TimeSpan? maxCacheAge = null;
        public TimeSpan? MaxCacheAge
        {
            get => this.maxCacheAge;
            set
            {
                this.maxCacheAge = value;
                foreach (var cacher in cachedContents.Values)
                    cacher.MaxAge = value;
            }
        }

        public T GetOrPopulate(String key, Func<T> populator) => cachedContents.GetOrAdd(key, new Cacher<T>(populator, maxCacheAge)).Value;
    }
}
