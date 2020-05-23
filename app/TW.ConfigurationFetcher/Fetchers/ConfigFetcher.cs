using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher.Fetcher
{
    class ConfigFetcher : IFetcher
    {
        public bool Overwrite { get; set; }
        public String DefaultTimeZoneId { get; set; } = "Europe/London";

        public override string Endpoint => "/interface.php?func=get_config";

        public override string Label => "config";

        public override bool NeedsUpdate(World world) => Overwrite || world.WorldSettings == null;

        private WorldSettings Parse(XmlParser xml)
        {
            return new WorldSettings
            {
                CanDemolishBuildings = xml.Get<bool>("/config/build/destroy"),
                AccountSittingEnabled = xml.Get<bool>("/config/sitter/allow"),
                ArchersEnabled = xml.Get<bool>("/config/game/archer"),
                BonusVillagesEnabled = xml.Get<int, bool>("/config/coord/bonus_villages", i => i > 0),
                ChurchesEnabled = xml.Get<bool>("/config/game/church"),
                // I think this one's right? There's no obvious XML property for flags
                FlagsEnabled = xml.Get<int, bool>("/config/game/event", i => i > 0),
                // No direct properties for noble loyalty drop range either
                NoblemanLoyaltyMin = 20,
                NoblemanLoyaltyMax = 35,
                GameSpeed = xml.Get<decimal>("/config/speed"),
                MaxNoblemanDistance = xml.Get<short>("/config/snob/max_dist"),
                // No direct properties for militia enabled?
                MilitiaEnabled = true,
                MillisecondsEnabled = xml.Get<bool>("/config/commands/millis_arrival"),
                // Yes XML has a typo, morale -> moral
                MoraleEnabled = xml.Get<int, bool>("/config/moral", i => i > 0),
                NightBonusEnabled = xml.Get<int, bool>("/config/night/active", i => i > 0),
                PaladinEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 0),
                PaladinSkillsEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 1),
                PaladinItemsEnabled = xml.Get<int, bool>("/config/game/knight_new_items", i => i > 0),
                UnitSpeed = xml.Get<decimal>("/config/unit_speed"),
                WatchtowerEnabled = xml.Get<int, bool>("/config/game/watchtower", i => i > 0),
                TimeZoneId = DefaultTimeZoneId
            };
        }

        private static Type[] ComparableSettingPropertyTypes = new Type[]
        {
            typeof(Int32),
            typeof(Int16),
            typeof(Boolean),
            typeof(Decimal),
            typeof(String),
        };

        private void CopyTo(WorldSettings source, WorldSettings target)
        {
            var properties = typeof(WorldSettings).GetProperties();
            foreach (var prop in properties)
            {
                if (!ComparableSettingPropertyTypes.Contains(prop.PropertyType) || prop.Name == "WorldId")
                    continue;

                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }

        public override void Process(VaultContext context, World world, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);
            var newSettings = Parse(xml);
            var oldSettings = world.WorldSettings;

            if (oldSettings == null)
            {
                Console.WriteLine("Got world settings for new world, storing");
                world.WorldSettings = newSettings;
                context.Add(world.WorldSettings);
                context.SaveChanges();
            }
            else if (Overwrite)
            {
                var props = typeof(WorldSettings).GetProperties();
                var differingValues = new Dictionary<String, object[]>();
                foreach (var prop in props)
                {
                    if (!ComparableSettingPropertyTypes.Contains(prop.PropertyType) || prop.Name == "WorldId")
                        continue;

                    var newValue = prop.GetValue(newSettings);
                    var oldValue = prop.GetValue(oldSettings);

                    if (!newValue.Equals(oldValue))
                        differingValues.Add(prop.Name, new[] { oldValue, newValue });
                }

                if (differingValues.Count == 0)
                {
                    Console.WriteLine("Got updated world settings for existing world, but stored config matches latest. No action necessary.");
                    return;
                }

                Console.WriteLine("Got updated world settings for existing world, and new settings do not match the old:");
                foreach (var prop in differingValues.Keys)
                {
                    var values = differingValues[prop];
                    var oldValue = values[0];
                    var newValue = values[1];

                    Console.WriteLine("- {0}: Changed from {1} (old) to {2} (new)", prop, oldValue, newValue);
                }

                bool? confirmOverwrite = null;
                while (!confirmOverwrite.HasValue)
                {
                    Console.Write("Overwrite? [y/n]: ");
                    var response = Console.ReadLine();
                    if (response.ToLower().Trim() == "y") confirmOverwrite = true;
                    if (response.ToLower().Trim() == "n") confirmOverwrite = false;
                }

                if (confirmOverwrite.Value)
                {
                    Console.WriteLine("Copying new config...");
                    CopyTo(newSettings, oldSettings);
                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("Ignoring new config, keeping old.");
                }
            }
        }
    }
}
