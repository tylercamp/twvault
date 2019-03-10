using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class BuildingLevels : Dictionary<String, short>
    {
        public BuildingLevels()
        {

        }

        public BuildingLevels(BuildingLevels source)
        {
            foreach (var kvp in source)
                this.Add(kvp.Key, kvp.Value);
        }

        public short this[Native.BuildingType buildingType]
        {
            get => this[buildingType.ToString().ToLower()];
            set => this[buildingType.ToString().ToLower()] = value;
        }

        public bool ContainsKey(Native.BuildingType buildingType)
        {
            return this.ContainsKey(buildingType.ToString().ToLower());
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

        public static bool operator ==(BuildingLevels a, BuildingLevels b)
        {
            bool aIsNull = ReferenceEquals(a, null);
            bool bIsNull = ReferenceEquals(b, null);

            if (aIsNull != bIsNull)
                return false;

            if (aIsNull)
                return true;

            if (a.Keys.Except(b.Keys).Any() || b.Keys.Except(a.Keys).Any())
                return false;

            foreach (var building in a.Keys)
            {
                if (a[building] != b[building])
                    return false;
            }

            return true;
        }

        public static bool operator !=(BuildingLevels a, BuildingLevels b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is BuildingLevels)
                return this == (obj as BuildingLevels);
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
