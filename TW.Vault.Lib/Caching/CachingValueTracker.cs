using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Caching
{
    public class CachingTimeTracker
    {
        DateTime populatedAt;

        public TimeSpan? MaxAge { get; set; }

        public bool IsExpired => MaxAge != null && DateTime.UtcNow - this.populatedAt > MaxAge.Value;

        public void MarkUpdated()
        {
            populatedAt = DateTime.UtcNow;
        }
    }
}
