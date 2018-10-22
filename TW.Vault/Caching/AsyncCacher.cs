using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class AsyncCacher<T> where T : class
    {
        T value;
        Func<object, Task<T>> populator;
        CachingTimeTracker tracker = new CachingTimeTracker();

        public AsyncCacher(Func<object, Task<T>> populator)
        {
            this.populator = populator;
        }

        public AsyncCacher(Func<object, Task<T>> populator, TimeSpan? maxAge)
        {
            this.populator = populator;
            this.MaxAge = maxAge;
        }

        public TimeSpan? MaxAge
        {
            get => tracker.MaxAge;
            set => tracker.MaxAge = value;
        }

        public bool IsExpired => this.value == null || tracker.IsExpired;

        public Task<T> Value(object context)
        {
            if (IsExpired)
                return Reload(context);
            else
                return Task.FromResult(value);
        }

        public async Task<T> Reload(object context)
        {
            this.value = await populator(context);
            tracker.MarkUpdated();
            return this.value;
        }
    }
}
