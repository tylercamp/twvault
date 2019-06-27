﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TW.Vault.Scaffold.Seed
{
    public static class WorldSettingsData
    {
        /*
         * Given exported CSV data:
         *
         * let csv = `...`;
         * let parsedEntries = csv.split('\n').filter(l => l.length > 0).splice(1).map(l => l.split(',')).map(e => ({ worldId: e[0], demolish: e[1].toLowerCase(), sitting: e[2].toLowerCase(), archers: e[3].toLowerCase(), bonusVillage: e[4].toLowerCase(), church: e[5].toLowerCase(), flags: e[6].toLowerCase(), nobleMin: e[7], nobleMax: e[8], loyaltyRate: e[9], gameSpeed: eval(e[10]), nobleDist: e[11], militia: e[12].toLowerCase(), millis: e[13].toLowerCase(), morale: e[14].toLowerCase(), night: e[15].toLowerCase(), pally: e[16].toLowerCase(), pallySkills: e[17].toLowerCase(), pallyItems: e[18].toLowerCase(), unitSpeed: eval(e[19]), watchtower: e[20].toLowerCase(), utcOffset: e[21], timezone: e[22] }));
         * parsedEntries.map(e => `new WorldSettings { WorldId = ${e.worldId}, CanDemolishBuildings = ${e.demolish}, AccountSittingEnabled = ${e.sitting}, ArchersEnabled = ${e.archers}, BonusVillagesEnabled = ${e.bonusVillage}, ChurchesEnabled = ${e.church}, FlagsEnabled = ${e.flags}, NoblemanLoyaltyMin = ${e.nobleMin}, NoblemanLoyaltyMax = ${e.nobleMax}, GameSpeed = ${e.gameSpeed}M, MaxNoblemanDistance = ${e.nobleDist}, MilitiaEnabled = ${e.militia}, MillisecondsEnabled = ${e.millis}, MoraleEnabled = ${e.morale}, NightBonusEnabled = ${e.night}, PaladinEnabled = ${e.pally}, PaladinSkillsEnabled = ${e.pallySkills}, PaladinItemsEnabled = ${e.pallyItems}, UnitSpeed = ${e.unitSpeed}M, WatchtowerEnabled = ${e.watchtower}, TimeZoneId = ${e.timezone} }`).join(',\n')
         */
        public static List<WorldSettings> Contents { get; } = new List<WorldSettings>
        {
            new WorldSettings { WorldId = 3, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 100, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 5, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = false, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1.8M, MaxNoblemanDistance = 100, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 0.6M, WatchtowerEnabled = false, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 6, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 2000, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = false, PaladinSkillsEnabled = false, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 8, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = false, BonusVillagesEnabled = false, ChurchesEnabled = false, FlagsEnabled = false, NoblemanLoyaltyMin = 25, NoblemanLoyaltyMax = 35, GameSpeed = 4M, MaxNoblemanDistance = 2000, MilitiaEnabled = false, MillisecondsEnabled = true, MoraleEnabled = false, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = false, PaladinItemsEnabled = false, UnitSpeed = 0.25M, WatchtowerEnabled = false, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 9, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = true, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 2M, MaxNoblemanDistance = 100, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 0.5M, WatchtowerEnabled = false, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 12, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = true, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 150, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 13, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = false, BonusVillagesEnabled = true, ChurchesEnabled = true, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1.3M, MaxNoblemanDistance = 2000, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 0.8M, WatchtowerEnabled = false, TimeZoneId = "America/New_York" },
            new WorldSettings { WorldId = 14, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = true, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 70, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "America/New_York" },
            new WorldSettings { WorldId = 15, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 1000, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 16, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = true, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1M, MaxNoblemanDistance = 1000, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 1M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" },
            new WorldSettings { WorldId = 17, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = true, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1.3M, MaxNoblemanDistance = 2000, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 0.7M, WatchtowerEnabled = false, TimeZoneId = "America/New_York" },
            new WorldSettings { WorldId = 18, CanDemolishBuildings = true, AccountSittingEnabled = true, ArchersEnabled = false, BonusVillagesEnabled = true, ChurchesEnabled = false, FlagsEnabled = true, NoblemanLoyaltyMin = 20, NoblemanLoyaltyMax = 35, GameSpeed = 1.8M, MaxNoblemanDistance = 150, MilitiaEnabled = true, MillisecondsEnabled = true, MoraleEnabled = true, NightBonusEnabled = false, PaladinEnabled = true, PaladinSkillsEnabled = true, PaladinItemsEnabled = false, UnitSpeed = 0.55M, WatchtowerEnabled = true, TimeZoneId = "Europe/London" }
        };
    }
}