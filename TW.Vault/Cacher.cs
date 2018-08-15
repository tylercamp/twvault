using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public class Cacher<T> where T : class
    {
        T value;
        DateTime populatedAt;
        Func<T> populator;

        public Cacher(Func<T> populator)
        {
            this.populator = populator;
        }

        public Cacher(Func<T> populator, TimeSpan? maxAge)
        {
            this.populator = populator;
            this.MaxAge = maxAge;
        }

        public TimeSpan? MaxAge { get; set; }

        public T Value
        {
            get
            {
                if (this.value == null)
                {
                    this.value = this.populator();
                    populatedAt = DateTime.UtcNow;
                }

                if (MaxAge != null)
                {
                    var now = DateTime.UtcNow;
                    var timeSinceCached = now - this.populatedAt;
                    if (timeSinceCached.Ticks >= MaxAge.Value.Ticks)
                    {
                        this.value = populator();
                        this.populatedAt = now;
                    }
                }

                return this.value;
            }
        }

        public void Reload()
        {
            this.value = populator();
            this.populatedAt = DateTime.UtcNow;
        }
    }
}
