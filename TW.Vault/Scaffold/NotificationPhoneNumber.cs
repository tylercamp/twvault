using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class NotificationPhoneNumber
    {
        public int Id { get; set; }
        public int Uid { get; set; }
        public string PhoneNumber { get; set; }
        public string Label { get; set; }
        public bool Enabled { get; set; }

        public User U { get; set; }
    }
}
