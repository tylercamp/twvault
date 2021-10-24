using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TW.Vault.Lib.Scaffold;

namespace TW.ConfigurationFetcher
{
    class PropertyDiff
    {
        public PropertyInfo Property { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public override string ToString()
        {
            return $"{Property.Name} ({OldValue}->{NewValue})";
        }
    }

    class DescriptorComparison
    {
        // Whether or not these descriptors effectively describe the same world
        // (if settings changed for an existing world, it would be a match with
        // a list of what changed.)
        public List<PropertyDiff> ChangedProperties { get; set; }
    }

    class WorldDescriptor
    {
        public delegate object SettingsSelector(WorldSettings settings);

        public String Hostname { get; set; }
        public short DefaultTranslationId { get; set; }
        public WorldSettings Settings { get; set; }

        public WorldDescriptor() { }
        
        public WorldDescriptor(World world)
        {
            this.Hostname = world.Hostname;
            this.DefaultTranslationId = world.DefaultTranslationId;
            this.Settings = world.WorldSettings;
        }


        private static Type[] ComparableSettingPropertyTypes = new Type[]
        {
            typeof(Int32),
            typeof(Int16),
            typeof(Boolean),
            typeof(Decimal),
            typeof(String),
        };

        public List<PropertyDiff> CompareTo(WorldDescriptor newDescriptor)
        {
            var result = new List<PropertyDiff>();
            var props = typeof(WorldSettings).GetProperties();
            foreach (var prop in props)
            {
                if (!ComparableSettingPropertyTypes.Contains(prop.PropertyType) || prop.Name == "WorldId")
                    continue;

                var newValue = prop.GetValue(newDescriptor.Settings);
                var oldValue = prop.GetValue(this.Settings);

                if (!newValue.Equals(oldValue))
                    result.Add(new PropertyDiff { Property = prop, OldValue = oldValue, NewValue = newValue });
            }

            return result;
        }

        public int EffectiveHash(Config config)
        {
            var rand = new Random(0);
            int hash = Hostname.GetHashCode() ^ rand.Next();
            foreach (var p in config.ResetOnDiff) hash ^= (p.GetValue(Settings).ToString().GetHashCode() ^ rand.Next());
            return hash;
        }
    }
}
