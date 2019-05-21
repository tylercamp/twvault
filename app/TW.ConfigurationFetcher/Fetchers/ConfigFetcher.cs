using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher.Fetcher
{
    class ConfigFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_config";

        public override string Label => "config";

        public override bool NeedsUpdate(World world) => world.WorldSettings == null;

        public override void Process(VaultContext context, World world, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);

            if (world.WorldSettings == null)
            {
                world.WorldSettings = new WorldSettings
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
                    NightBonusEnabled = xml.Get<bool>("/config/night/active"),
                    PaladinEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 0),
                    PaladinSkillsEnabled = xml.Get<int, bool>("/config/game/knight", i => i > 1),
                    PaladinItemsEnabled = xml.Get<int, bool>("/config/game/knight_new_items", i => i > 0),
                    UnitSpeed = xml.Get<decimal>("/config/unit_speed"),
                    WatchtowerEnabled = xml.Get<int, bool>("/config/game/watchtower", i => i > 0)
                };

                context.Add(world.WorldSettings);
                context.SaveChanges();
            }
        }
    }
}
