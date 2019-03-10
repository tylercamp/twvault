using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class InvalidDataRecord
    {
        public long Id { get; set; }
        public string Endpoint { get; set; }
        public string Reason { get; set; }
        public int UserId { get; set; }
        public string DataString { get; set; }

        public User User { get; set; }
    }
}
