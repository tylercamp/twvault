using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Convert
{
    public static class PlayerConvert
    {
        public static JSON.Player ModelToJson(Scaffold.Player player)
        {
            return new JSON.Player
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                PlayerRank = player.PlayerRank,
                Points = player.Points,
                TribeId = player.TribeId,
                Villages = player.Villages
            };
        }
    }
}
