using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class NotificationRequest
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public DateTime EventOccursAt { get; set; }
        public int Uid { get; set; }

        public User U { get; set; }
    }
}
