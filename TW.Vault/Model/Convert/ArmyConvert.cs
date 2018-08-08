using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scaffold = TW.Vault.Scaffold_Model;
using JSON = TW.Vault.Model.JSON;
using static TW.Vault.Model.Convert.ConvertUtil;

namespace TW.Vault.Model.Convert
{
    public static class ArmyConvert
    {
        private static Random ArmyRandom = new Random();

        private static short ToShort(this int value)
        {
            return (short)value;
        }

        public static T JsonToArmy<T>(JSON.Army armyCounts, T existingArmy = null, Scaffold.VaultContext context = null) where T : class, new()
        {
            if (armyCounts == null || armyCounts.Count == 0)
            {
                if (existingArmy != null && context != null)
                    context.Remove(existingArmy);

                return null;
            }

            T result;
            if (existingArmy != null)
            {
                result = existingArmy;
            }
            else
            {
                result = new T();
                if (context != null)
                    context.Add(result);
            }

            var scaffoldArmyType = typeof(T);
            foreach (String troopType in Enum.GetNames(typeof(JSON.TroopType)))
            {
                String lowerName = troopType.ToLower();

                var troopCount = GetOrNull(armyCounts, lowerName);
                var troopProperty = scaffoldArmyType.GetProperty(troopType, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (typeof(short?).IsAssignableFrom(troopProperty.PropertyType))
                {
                    troopProperty.SetValue(result, troopCount?.ToShort());
                }
                else
                {
                    troopProperty.SetValue(result, troopCount);
                }
            }

            return result;
        }

        public static JSON.Army ArmyToJson<T>(T army) where T : class
        {
            if (army == null)
                return null;

            var result = new JSON.Army();

            var scaffoldArmyType = army.GetType();
            foreach (String troopType in Enum.GetNames(typeof(JSON.TroopType)))
            {
                String lowerName = troopType.ToLower();

                var troopProperty = scaffoldArmyType.GetProperty(troopType, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var nullableTroopCount = troopProperty.GetValue(army);

                if (nullableTroopCount != null)
                {
                    int troopCount;
                    if (typeof(short?).IsAssignableFrom(troopProperty.PropertyType))
                        troopCount = ((short?)troopProperty.GetValue(army)).Value;
                    else
                        troopCount = ((int?)troopProperty.GetValue(army)).Value;

                    result.Add(lowerName, troopCount);
                }
            }

            return result;
        }
    }
}
