using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;
using static TW.Vault.Model.Convert.ConvertUtil;

namespace TW.Vault.Model.Convert
{
    public static class ArmyConvert
    {
        private static short ToShort(this int value)
        {
            return (short)value;
        }

        public static Scaffold.CommandArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.CommandArmy existingArmy = null, Scaffold.VaultContext context = null) =>
            JsonToArmy<Scaffold.CommandArmy>(armyCounts, worldId, existingArmy, context);

        public static Scaffold.ReportArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.ReportArmy existingArmy = null, Scaffold.VaultContext context = null) =>
            JsonToArmy<Scaffold.ReportArmy>(armyCounts, worldId, existingArmy, context);

        public static Scaffold.CurrentArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.CurrentArmy existingArmy = null, Scaffold.VaultContext context = null) =>
            JsonToArmy<Scaffold.CurrentArmy>(armyCounts, worldId, existingArmy, context);

        private static T JsonToArmy<T>(JSON.Army armyCounts, short worldId, T existingArmy = null, Scaffold.VaultContext context = null, Action<T> keyPopulator = null) where T : class, new()
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
                keyPopulator?.Invoke(result);
            }

            var scaffoldArmyType = typeof(T);
            foreach (String troopType in Enum.GetNames(typeof(JSON.TroopType)))
            {
                String lowerName = troopType.ToLower();

                var troopCount = GetOrNull(armyCounts, lowerName);
                var troopProperty = scaffoldArmyType.GetProperty(troopType, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var currentCount = (int?)troopProperty.GetValue(result);

                if (currentCount == troopCount)
                    continue;
                
                if (typeof(short?).IsAssignableFrom(troopProperty.PropertyType))
                    troopProperty.SetValue(result, troopCount?.ToShort());
                else
                    troopProperty.SetValue(result, troopCount);
            }

            var worldIdProperty = scaffoldArmyType.GetProperty("WorldId", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (worldIdProperty != null)
                worldIdProperty.SetValue(result, worldId);

            if (existingArmy == null && context != null)
                context.Add(result);

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

        public static JSON.TroopType StringToType(String troopType)
        {
            if (!Char.IsUpper(troopType[0]))
                troopType = Char.ToUpper(troopType[0]) + troopType.Substring(1);

            return Enum.Parse<JSON.TroopType>(troopType);
        }

        public static String TypeToString(JSON.TroopType troopType) => troopType.ToString().ToLower();
    }
}
