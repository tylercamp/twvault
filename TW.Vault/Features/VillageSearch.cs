﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features
{
    public class VillageSearch
    {
        public struct Query
        {
            public int WorldId;
            public List<String> PlayerNames;
            public List<String> TribeNamesOrTags;
            public List<String> Continents;
        }

        public static async Task<String> ListCoords(Scaffold.VaultContext context, Query query)
        {
            var players = new List<String>(query.PlayerNames);
            var tribes = query.TribeNamesOrTags;
            var worldId = query.WorldId;
            var continents = query.Continents;

            players.AddRange(await (
                    from tribe in context.Ally.FromWorld(worldId)
                    join player in context.Player.FromWorld(worldId) on tribe.TribeId equals player.TribeId
                    where tribes.Contains(tribe.TribeName) || tribes.Contains(tribe.Tag)
                    select player.PlayerName
                ).ToListAsync());

            var playerIds = await context.Player
                .Where(p => players.Contains(p.PlayerName))
                .Select(p => p.PlayerId)
                .ToListAsync();

            var villageCoords = await context.Village
                .Where(v => playerIds.Contains(v.PlayerId.Value))
                .Select(v => new { v.X, v.Y })
                .ToListAsync();

            if (continents.Count > 0)
            {
                villageCoords = villageCoords.Where(coord => continents.Any(k =>
                {
                    var xmin = int.Parse(k[1].ToString()) * 100;
                    var ymin = int.Parse(k[0].ToString()) * 100;

                    return
                        coord.X >= xmin && coord.X < xmin + 100 &&
                        coord.Y >= ymin && coord.Y < ymin + 100;
                })).ToList();
            }

            var coordString = String.Join(' ', villageCoords.Select(c => $"{c.X}|{c.Y}"));
            return coordString;
        }
    }
}
