using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class AsyncCacher<T> where T : class
    {
        T value;
        Func<Task<T>> populator;
        CachingTimeTracker tracker = new CachingTimeTracker();

        public AsyncCacher(Func<Task<T>> populator)
        {
            this.populator = populator;
        }

        public AsyncCacher(Func<Task<T>> populator, TimeSpan? maxAge)
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

        public Task<T> Value
        {
            get
            {
                if (IsExpired)
                    return Reload();
                else
                    return Task.FromResult(value);
            }
        }

        public async Task<T> Reload()
        {
            this.value = await populator();
            tracker.MarkUpdated();
            return this.value;
        }
    }
}
