using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class UserUploadHistory
    {
        public long Id { get; set; }
        public int Uid { get; set; }
        public DateTime? LastUploadedReportsAt { get; set; }
        public DateTime? LastUploadedIncomingsAt { get; set; }
        public DateTime? LastUploadedCommandsAt { get; set; }
        public DateTime? LastUploadedTroopsAt { get; set; }

        public User U { get; set; }
    }
}
