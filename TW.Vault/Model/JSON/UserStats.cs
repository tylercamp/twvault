using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class UserStats
    {
        public float DVsAtHome { get; set; }
        public float BacklineDVsAtHome { get; set; }
        public int NukesInPastWeek { get; set; }
        public int FangsInPastWeek { get; set; }
        public int FakesInPastWeek { get; set; }
        public Dictionary<String, long> PopPerTribe { get; set; }
    }
}
