using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Convert
{
    public static class VillageConvert
    {
        public static JSON.Village ModelToJson(Scaffold.Village village)
        {
            return new JSON.Village
            {
                PlayerId = village.PlayerId,
                Points = village.Points,
                VillageId = village.VillageId,
                VillageName = village.VillageName,
                VillageRank = village.VillageRank,
                X = village.X,
                Y = village.Y
            };
        }
    }
}
