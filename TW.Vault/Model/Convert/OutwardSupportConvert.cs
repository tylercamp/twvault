using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class OutwardSupportConvert
    {
        public static Scaffold.CurrentVillageSupport ToModel(
                long sourceVillageId,
                JSON.PlayerOutwardSupport.SupportedVillage villageData,
                Scaffold.CurrentVillageSupport existingSupport = null,
                Scaffold.VaultContext context = null
            )
        {
            if (existingSupport == null)
            {
                existingSupport = new Scaffold.CurrentVillageSupport();
                if (context != null)
                    context.CurrentVillageSupport.Add(existingSupport);
            }

            existingSupport.SourceVillageId = sourceVillageId;
            existingSupport.TargetVillageId = villageData.Id;

            existingSupport.LastUpdatedAt = DateTime.UtcNow;
            existingSupport.SupportingArmy = ArmyConvert.JsonToArmy(villageData.TroopCounts, existingSupport.SupportingArmy, context);

            return existingSupport;
        }
    }
}
