using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class PerformanceRecord
    {
        public long Id { get; set; }
        public string OperationLabel { get; set; }
        public TimeSpan AverageTime { get; set; }
        public TimeSpan MinTime { get; set; }
        public TimeSpan MaxTime { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int NumSamples { get; set; }
    }
}
