﻿using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold_Model
{
    public partial class CurrentArmy
    {
        public CurrentArmy()
        {
            CurrentVillageArmyOwned = new HashSet<CurrentVillage>();
            CurrentVillageArmyStationed = new HashSet<CurrentVillage>();
            CurrentVillageArmyTraveling = new HashSet<CurrentVillage>();
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

        public ICollection<CurrentVillage> CurrentVillageArmyOwned { get; set; }
        public ICollection<CurrentVillage> CurrentVillageArmyStationed { get; set; }
        public ICollection<CurrentVillage> CurrentVillageArmyTraveling { get; set; }
    }
}
