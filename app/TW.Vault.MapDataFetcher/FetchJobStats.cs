using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.MapDataFetcher
{
    public class FetchWorldJobStats
    {
        public int NumVillagesUpdated { get; set; }
        public int NumVillagesCreated { get; set; }
        public int NumTribesUpdated { get; set; }
        public int NumTribesCreated { get; set; }
        public int NumConquersCreated { get; set; }
        public int NumPlayersUpdated { get; set; }
        public int NumPlayersCreated { get; set; }
    }

    public class FetchJobStats
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration { get; set; }

        public int TotalVillagesUpdated { get; set; }
        public int TotalTribesUpdated { get; set; }
        public int TotalConquersUpdated { get; set; }
        public int TotalPlayersUpdated { get; set; }

        public Dictionary<String, FetchWorldJobStats> StatsByWorld { get; set; } = new Dictionary<string, FetchWorldJobStats>();
    }
}
