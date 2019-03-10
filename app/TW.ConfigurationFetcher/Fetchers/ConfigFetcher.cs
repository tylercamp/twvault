using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TW.ConfigurationFetcher.Fetcher
{
    class ConfigFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_config";

        public override string Label => "config";

        public override void Process(String source, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);

            File.WriteAllText($"{source}_config.csv", new XmlToCsvWriter(xml)
                .AddSimpleColumn<bool>("can_demolish_buildings", "/config/build/destroy")
                .AddSimpleColumn<bool>("account_sitting_enabled", "/config/sitter/allow")
                .AddSimpleColumn<bool>("archers_enabled", "/config/game/archer")
                .AddSimpleColumn<bool>("bonus_villages_enabled", "/config/coord/bonus_villages")
                .AddSimpleColumn<bool>("churches_enabled", "/config/game/church")
                // I think this one's right? There's no obvious XML property for flags
                .AddSimpleColumn<int>("flags_enabled", "/config/game/event", IntToBool)
                // No direct properties for noble loyalty drop range either
                .AddDirectSimpleColumn("nobleman_loyalty_min", 20)
                .AddDirectSimpleColumn("nobleman_loyalty_max", 35)
                // Loyalty is generally 1 per hour, augmented by world speed
                .AddSimpleColumn("loyalty_per_hour", "/config/speed")
                .AddSimpleColumn("game_speed", "/config/speed")
                .AddSimpleColumn("max_nobleman_distance", "/config/snob/max_dist")
                // No direct properties for militia enabled?
                .AddDirectSimpleColumn("militia_enabled", true)
                .AddSimpleColumn<bool>("milliseconds_enabled", "/config/commands/millis_arrival")
                //  Yes XML has a typo, morale -> moral
                .AddSimpleColumn<int>("morale_enabled", "/config/moral", IntToBool)
                .AddSimpleColumn<bool>("night_bonus_enabled", "/config/night/active")
                .AddSimpleColumn<int>("paladin_enabled", "/config/game/knight", IntToBool)
                .AddSimpleColumn<int>("paladin_skills_enabled", "/config/game/knight", (l) => l > 1)
                .AddSimpleColumn("paladin_items_enabled", "/config/game/knight_new_items", (v) => v == "1")
                .AddSimpleColumn("unit_speed", "/config/unit_speed")
                .AddSimpleColumn<int>("watchtower_enabled", "/config/game/watchtower", IntToBool)
                .ToString()
            );
        }
    }
}
