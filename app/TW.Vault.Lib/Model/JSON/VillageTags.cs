using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class VillageTags
    {
        public bool IsStacked { get; set; }
        public float? StackDVs { get; set; }
        public DateTime? StackSeenAt { get; set; }

        public bool HasNuke { get; set; }
        public DateTime? NukeSeenAt { get; set; }

        public bool HasNobles { get; set; }
        public DateTime? NoblesSeenAt { get; set; }

        public bool HasDefense { get; set; }
        public DateTime? DefenseSeenAt { get; set; }

        public int? WallLevel { get; set; }
        public DateTime? WallLevelSeenAt { get; set; }

        public int? WatchtowerLevel { get; set; }
        public DateTime? WatchtowerSeenAt { get; set; }

        public int NumTargettingNukes { get; set; }

        public int? ReturningTroopsPopulation { get; set; }

        public String TribeName { get; set; }
        public bool IsEnemyTribe { get; set; }
    }
}
