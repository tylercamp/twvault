using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class CommandArmy : IScaffoldArmy
    {
        public CommandArmy()
        {
            Command = new HashSet<Command>();
        }

        public long ArmyId { get; set; }
        public int? Spear { get; set; }
        public int? Sword { get; set; }
        public int? Axe { get; set; }
        public int? Archer { get; set; }
        public int? Spy { get; set; }
        public int? Light { get; set; }
        public int? Marcher { get; set; }
        public int? Heavy { get; set; }
        public int? Ram { get; set; }
        public int? Catapult { get; set; }
        public int? Knight { get; set; }
        public int? Snob { get; set; }
        public int? Militia { get; set; }
        public short WorldId { get; set; }

        public World World { get; set; }
        public ICollection<Command> Command { get; set; }
    }
}
