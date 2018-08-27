using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class PlayerSummary
    {
        public long PlayerId { get; set; }
        public String PlayerName { get; set; }
        public int? MaxPossibleNobles { get; set; }
        public List<Army> Armies { get; set; }
        public TimeSpan UploadAge { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
