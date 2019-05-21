using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class PlayerSummary
    {
        /* General info */
        public long PlayerId { get; set; }
        public String PlayerName { get; set; }
        public String TribeName { get; set; }

        /* Nobles */
        public int? MaxPossibleNobles { get; set; }
        public int NumNobles { get; set; }

        /* Upload history */
        public TimeSpan UploadAge { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime UploadedIncomingsAt { get; set; }
        public DateTime UploadedCommandsAt { get; set; }
        public DateTime UploadedReportsAt { get; set; }

        /* Off/Def village estimates */
        public int NumOffensiveVillages { get; set; }
        public int NumDefensiveVillages { get; set; }

        /* Off stats */
        public float QuarterNukesOwned { get; set; }
        public float HalfNukesOwned { get; set; }
        public float ThreeQuarterNukesOwned { get; set; }
        public float NukesOwned { get; set; }
        public float NukesTraveling { get; set; }
        public int FangsOwned { get; set; }
        public int FangsTraveling { get; set; }

        /* Def stats */
        public Dictionary<String, int> SupportPopulationByTargetTribe { get; set; }

        public float DVsAtHome { get; set; }
        public float DVsAtHomeBackline { get; set; }
        public float DVsOwned { get; set; }
        public float DVsSupportingSelf { get; set; }
        public float DVsSupportingOthers { get; set; }
        public float DVsTraveling { get; set; }


        /* Command stats */
        public int NumAttackCommands { get; set; }
        public int NumSupportCommands { get; set; }
        public int NumIncomings { get; set; }
    }
}
