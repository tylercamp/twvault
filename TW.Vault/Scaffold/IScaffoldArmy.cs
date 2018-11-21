using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Scaffold
{
    public interface IScaffoldArmy
    {
        int? Spear { get; set; }
        int? Sword { get; set; }
        int? Axe { get; set; }
        int? Archer { get; set; }
        int? Spy { get; set; }
        int? Light { get; set; }
        int? Marcher { get; set; }
        int? Heavy { get; set; }
        int? Ram { get; set; }
        int? Catapult { get; set; }
        int? Knight { get; set; }
        int? Snob { get; set; }
        int? Militia { get; set; }
    }
}
