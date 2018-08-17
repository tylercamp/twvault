using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class BuildingLevels : Dictionary<String, short>
    {
        public short this[Native.BuildingType buildingType]
        {
            get => this[buildingType.ToString().ToLower()];
            set => this[buildingType.ToString().ToLower()] = value;
        }

        public short GetValueOrDefault(Native.BuildingType buildingType, short defaultValue)
        {
            if (this.ContainsKey(buildingType.ToString().ToLower()))
                return this[buildingType];
            else
                return defaultValue;
        }

        public short GetValueOrMax(Native.BuildingType buildingType)
        {
            if (this.ContainsKey(buildingType.ToString().ToLower()))
                return this[buildingType];
            else
                return Native.BuildingStats.MaxLevels[buildingType];
        }
    }
}
