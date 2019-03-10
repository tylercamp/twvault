using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class Cacher<T> where T : class
    {
        T value;
        Func<T> populator;
        CachingTimeTracker tracker = new CachingTimeTracker();

        public Cacher(Func<T> populator)
        {
            this.populator = populator;
        }

        public Cacher(Func<T> populator, TimeSpan? maxAge)
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

        public T Value
        {
            get
            {
                if (IsExpired)
                    Reload();

                return this.value;
            }
        }

        public T Reload()
        {
            this.value = populator();
            tracker.MarkUpdated();
            return this.value;
        }
    }
}
