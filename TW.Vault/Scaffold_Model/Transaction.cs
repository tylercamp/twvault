using System;
using System.Collections.Generic;
using System.Net;

namespace TW.Vault.Scaffold_Model
{
    public partial class Transaction
    {
        public Transaction()
        {
            Command = new HashSet<Command>();
            Report = new HashSet<Report>();
        }

        public long TxId { get; set; }
        public int Uid { get; set; }
        public DateTime OccurredAt { get; set; }
        public IPAddress Ip { get; set; }

        public ICollection<Command> Command { get; set; }
        public ICollection<Report> Report { get; set; }
    }
}
