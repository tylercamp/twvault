using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features.Simulation
{
    public class LoyaltyCalculator
    {
        private decimal loyaltyPerHour;

        public LoyaltyCalculator(decimal loyaltyPerHour)
        {
            this.loyaltyPerHour = loyaltyPerHour;
        }

        public short PossibleLoyalty(short oldLoyalty, TimeSpan timeSinceLoyalty)
        {
            var result = (short)Math.Min(100, Math.Abs(oldLoyalty) + timeSinceLoyalty.TotalHours * (double)loyaltyPerHour);

            return result;
        }
    }
}
