using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class AsyncCachingMap<T> where T : class
    {
        ConcurrentDictionary<String, AsyncCacher<T>> cachedContents = new ConcurrentDictionary<string, AsyncCacher<T>>();

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

        public Task<T> GetOrPopulate(String key, object context, Func<object, Task<T>> populator)
        {
            return cachedContents.GetOrAdd(key, new AsyncCacher<T>(populator, maxCacheAge)).Value(context);
        }
    }
}
