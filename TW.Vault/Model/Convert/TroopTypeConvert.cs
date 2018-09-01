using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class TroopTypeConvert
    {
        public static String ToTroopString(this TroopType troopType) => troopType.ToString().ToLower();

        public static TroopType ToTroopType(this String troopString) => StringToTroopType(troopString).Value;

        public static String TroopTypeToString(TroopType? troopType)
        {
            return troopType?.ToString()?.ToLower();
        }

        public static TroopType? StringToTroopType(String troopType)
        {
            if (troopType == null)
                return null;

            if (!char.IsUpper(troopType[0]))
                troopType = char.ToUpper(troopType[0]) + troopType.Substring(1);

            return Enum.Parse<TroopType>(troopType);
        }
    }
}
