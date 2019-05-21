using NodaTime;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class WorldSettings
    {
        public short WorldId { get; set; }
        public bool CanDemolishBuildings { get; set; }
        public bool AccountSittingEnabled { get; set; }
        public bool ArchersEnabled { get; set; }
        public bool BonusVillagesEnabled { get; set; }
        public bool ChurchesEnabled { get; set; }
        public bool FlagsEnabled { get; set; }
        public short NoblemanLoyaltyMin { get; set; }
        public short NoblemanLoyaltyMax { get; set; }
        public decimal GameSpeed { get; set; }
        public short MaxNoblemanDistance { get; set; }
        public bool MilitiaEnabled { get; set; }
        public bool MillisecondsEnabled { get; set; }
        public bool MoraleEnabled { get; set; }
        public bool NightBonusEnabled { get; set; }
        public bool PaladinEnabled { get; set; }
        public bool PaladinSkillsEnabled { get; set; }
        public bool PaladinItemsEnabled { get; set; }
        public decimal UnitSpeed { get; set; }
        public bool WatchtowerEnabled { get; set; }
        public String TimeZoneId { get; set; }

        public DateTime ServerTime
        {
            get
            {
                var timeZone = DateTimeZoneProviders.Tzdb[TimeZoneId];
                var zonedClock = SystemClock.Instance.InZone(timeZone);
                var zonedDateTime = zonedClock.GetCurrentZonedDateTime();
                return zonedDateTime.ToDateTimeUnspecified();
            }
        }

        public World World { get; set; }
    }
}
