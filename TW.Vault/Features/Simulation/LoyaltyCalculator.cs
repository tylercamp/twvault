using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features.Simulation
{
    public class LoyaltyCalculator
    {
        private int loyaltyPerHour = 2;

        public LoyaltyCalculator()
        {

        }

        public LoyaltyCalculator(int loyaltyPerHour)
        {

        }

        public short PossibleLoyalty(short oldLoyalty, TimeSpan timeSinceLoyalty)
        {
            var result = (short)Math.Min(100, Math.Abs(oldLoyalty) + timeSinceLoyalty.TotalHours * loyaltyPerHour);

            return result;
        }
    }
}
