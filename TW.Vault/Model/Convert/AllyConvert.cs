using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Convert
{
    public static class AllyConvert
    {
        public static JSON.Ally ModelToJson(Scaffold.Ally ally)
        {
            return new JSON.Ally
            {
                AllPoints = ally.AllPoints,
                Members = ally.Members,
                Points = ally.Points,
                Tag = ally.Tag,
                TribeId = ally.TribeId,
                TribeName = ally.TribeName,
                TribeRank = ally.TribeRank,
                Villages = ally.Villages
            };
        }
    }
}
