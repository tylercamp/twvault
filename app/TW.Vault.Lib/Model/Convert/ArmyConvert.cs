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

        public static Scaffold.CommandArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.CommandArmy existingArmy = null, Scaffold.VaultContext context = null, bool emptyIfNull = false) =>
            JsonToArmy<Scaffold.CommandArmy>(armyCounts, worldId, existingArmy, context, emptyIfNull);

        public static Scaffold.ReportArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.ReportArmy existingArmy = null, Scaffold.VaultContext context = null, bool emptyIfNull = false) =>
            JsonToArmy<Scaffold.ReportArmy>(armyCounts, worldId, existingArmy, context, emptyIfNull);

        public static Scaffold.CurrentArmy JsonToArmy(JSON.Army armyCounts, short worldId, Scaffold.CurrentArmy existingArmy = null, Scaffold.VaultContext context = null, bool emptyIfNull = false) =>
            JsonToArmy<Scaffold.CurrentArmy>(armyCounts, worldId, existingArmy, context, emptyIfNull);

        private static T JsonToArmy<T>(JSON.Army armyCounts, short worldId, T existingArmy = null, Scaffold.VaultContext context = null, bool emptyIfNull = false) where T : class, new()
        {
            if (Object.ReferenceEquals(armyCounts, null))
            {
                if (emptyIfNull)
                {
                    armyCounts = JSON.Army.Empty;
                }
                else
                {
                    if (existingArmy != null && context != null)
                        context.Remove(existingArmy);

                    return null;
                }
            }

            T result;
            if (existingArmy != null)
                result = existingArmy;
            else
                result = new T();

            var scaffoldArmyType = typeof(T);
            foreach (var troopType in Enum.GetValues(typeof(JSON.TroopType)).Cast<JSON.TroopType>())
            {
                var troopCount = GetOrNull(armyCounts, troopType);
                var troopProperty = scaffoldArmyType.GetProperty(troopType.ToString(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
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

        public static JSON.Army ArmyToJson(Scaffold.IScaffoldArmy army)
        {
            if (army == null)
                return null;

            var result = new JSON.Army();

            if (army.Spear != null) result[JSON.TroopType.Spear] = army.Spear.Value;
            if (army.Sword != null) result[JSON.TroopType.Sword] = army.Sword.Value;
            if (army.Axe != null) result[JSON.TroopType.Axe] = army.Axe.Value;
            if (army.Archer != null) result[JSON.TroopType.Archer] = army.Archer.Value;
            if (army.Spy != null) result[JSON.TroopType.Spy] = army.Spy.Value;
            if (army.Light != null) result[JSON.TroopType.Light] = army.Light.Value;
            if (army.Marcher != null) result[JSON.TroopType.Marcher] = army.Marcher.Value;
            if (army.Heavy != null) result[JSON.TroopType.Heavy] = army.Heavy.Value;
            if (army.Ram != null) result[JSON.TroopType.Ram] = army.Ram.Value;
            if (army.Catapult != null) result[JSON.TroopType.Catapult] = army.Catapult.Value;
            if (army.Knight != null) result[JSON.TroopType.Knight] = army.Knight.Value;
            if (army.Snob != null) result[JSON.TroopType.Snob] = army.Snob.Value;
            if (army.Militia != null) result[JSON.TroopType.Militia] = army.Militia.Value;

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
