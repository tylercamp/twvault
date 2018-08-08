using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TW.Vault.Model.Calculations
{
    public static class TroopCalculations
    {
        public static JSON.Army SubtractJson(JSON.Army left, JSON.Army right)
        {
            var result = new JSON.Army();

            foreach (var troopType in left.Keys)
            {
                var leftCount = left[troopType];
                var rightCount = right.GetValueOrDefault(troopType, 0);

                result.Add(troopType, leftCount - rightCount);
            }

            return result;
        }
    }
}
