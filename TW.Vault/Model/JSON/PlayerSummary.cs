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
        public TimeSpan UploadAge { get; set; }
        public DateTime UploadedAt { get; set; }

        //  All armies associated with the player (own troops and other players' troops that are stacking them)
        public Army ArmySupportingSelf { get; set; }

        public Army ArmyOwned { get; set; }
        public Army ArmyTraveling { get; set; }
        public Army ArmyAtHome { get; set; }
        public Army ArmySupportingOthers { get; set; }

        public int NumAttackCommands { get; set; }
        public int NumSupportCommands { get; set; }
        public int NumIncomings { get; set; }
    }
}
