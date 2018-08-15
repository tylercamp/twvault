using System;
using System.Collections.Generic;
using System.Net;

namespace TW.Vault.Scaffold
{
    public partial class FailedAuthorizationRecord
    {
        public IPAddress Ip { get; set; }
        public long? PlayerId { get; set; }
        public long? TribeId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string RequestedEndpoint { get; set; }
        public long Id { get; set; }
        public string Reason { get; set; }
        public short WorldId { get; set; }
    }
}
