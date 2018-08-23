using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public enum TroopType
    {
        [EnumMember(Value = "spear")]
        Spear,
        [EnumMember(Value = "sword")]
        Sword,
        [EnumMember(Value = "axe")]
        Axe,
        [EnumMember(Value = "archer")]
        Archer,
        [EnumMember(Value = "spy")]
        Spy,
        [EnumMember(Value = "light")]
        Light,
        [EnumMember(Value = "marcher")]
        Marcher,
        [EnumMember(Value = "heavy")]
        Heavy,
        [EnumMember(Value = "ram")]
        Ram,
        [EnumMember(Value = "catapult")]
        Catapult,
        [EnumMember(Value = "knight")]
        Knight,
        [EnumMember(Value = "snob")]
        Snob,
        [EnumMember(Value = "militia")]
        Militia
    }

    public enum UnitType
    {
        Infantry,
        Cavalry,
        Archer
    }

    public enum UnitBuild
    {
        Defensive,
        Offensive,
        Neutral
    }
}
