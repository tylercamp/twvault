using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TW.Vault.Model.Native
{
    public enum BuildingType
    {
        [EnumMember(Value = "barracks")]
        Barracks,
        [EnumMember(Value = "church")]
        Church,
        [EnumMember(Value = "farm")]
        Farm,
        [EnumMember(Value = "garage")]
        Garage,
        [EnumMember(Value = "hide")]
        Hide,
        [EnumMember(Value = "iron")]
        Iron,
        [EnumMember(Value = "main")]
        Main,
        [EnumMember(Value = "market")]
        Market,
        [EnumMember(Value = "place")]
        Place,
        [EnumMember(Value = "smith")]
        Smith,
        [EnumMember(Value = "snob")]
        Snob,
        [EnumMember(Value = "stable")]
        Stable,
        [EnumMember(Value = "statue")]
        Statue,
        [EnumMember(Value = "stone")]
        Stone,
        [EnumMember(Value = "storage")]
        Storage,
        [EnumMember(Value = "wall")]
        Wall,
        [EnumMember(Value = "watchtower")]
        Watchtower,
        [EnumMember(Value = "wood")]
        Wood
    }
}
