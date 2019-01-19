using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;

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
            public Coordinate? MinCoord, MaxCoord;
            public Coordinate? CenterCoord;
            public float? MaxDistance;
        }

        public static async Task<String> ListCoords(Scaffold.VaultContext context, Query query, bool randomize = true)
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

            var playerIds = await context.Player.FromWorld(worldId)
                .Where(p => players.Contains(p.PlayerName))
                .Select(p => p.PlayerId)
                .ToListAsync();

            var villageCoords = await context.Village.FromWorld(worldId)
                .Where(v => playerIds.Contains(v.PlayerId.Value))
                .Select(v => new { X = v.X.Value, Y = v.Y.Value, v.VillageId })
                .OrderBy(v => v.VillageId)
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

            if (query.MinCoord != null)
            {
                var minCoord = query.MinCoord.Value;
                villageCoords = villageCoords.Where(coord => coord.X >= minCoord.X && coord.Y >= minCoord.Y).ToList();
            }

            if (query.MaxCoord != null)
            {
                var maxCoord = query.MaxCoord.Value;
                villageCoords = villageCoords.Where(coord => coord.X <= maxCoord.X && coord.Y <= maxCoord.Y).ToList();
            }

            if (query.CenterCoord != null && query.MaxDistance != null)
            {
                var centerCoord = query.CenterCoord.Value;
                var maxDistance = query.MaxDistance.Value;

                villageCoords = villageCoords.Where(coord => centerCoord.DistanceTo(coord.X, coord.Y) <= maxDistance).ToList();
            }

            if (randomize)
            {
                var random = new System.Random(0);
                var ranked = villageCoords.Select(c => new { Rank = random.Next(), Coord = c }).OrderBy(r => r.Rank).Select(r => r.Coord);
                villageCoords = ranked.ToList();
            }

            var coordString = String.Join(' ', villageCoords.Select(c => $"{c.X}|{c.Y}"));
            return coordString;
        }
    }
}
