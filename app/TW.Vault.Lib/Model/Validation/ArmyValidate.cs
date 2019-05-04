using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;

namespace TW.Vault.Model.Validation
{
    public static class ArmyValidate
    {
        public static bool MeetsPopulationRestrictions(JSON.Army army, int populationCap = 32000)
        {
            if (army == null)
                return true;

            int totalPop = 0;
            foreach (var kvp in army)
                totalPop += Native.ArmyStats.Population[kvp.Key] * kvp.Value;

            return totalPop < populationCap;
        }
    }
}
