using System;
using System.Collections.Generic;
using System.Text;

namespace TW.Vault.Scaffold.Seed
{
    public static class WorldData
    {
        /*
         * Given exported CSV data:
         *
         * let csv = `...`;
         * let parsedEntries = csv.split('\n').splice(1).map(l => l.split(',')).map(e => ({ id: e[0], name: e[1], hostname: e[2], tid: e[3] }))
         * parsedEntries.map(e => `new World { Id = ${e.id}, Name = ${e.name}, Hostname = ${e.hostname}, DefaultTranslationId = ${e.tid} }`).join(',\n')
         */

        public static List<World> Contents { get; } = new List<World>
        {
            new World { Id = 3, Name = "en103", Hostname = "en103.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 5, Name = "en104", Hostname = "en104.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 6, Name = "en105", Hostname = "en105.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 8, Name = "enc1", Hostname = "enc1.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 9, Name = "en106", Hostname = "en106.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 12, Name = "en107", Hostname = "en107.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 13, Name = "us38", Hostname = "us38.tribalwars.us", DefaultTranslationId = 1 },
            new World { Id = 14, Name = "us40", Hostname = "us40.tribalwars.us", DefaultTranslationId = 1 },
            new World { Id = 15, Name = "enp6", Hostname = "enp6.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 16, Name = "enp5", Hostname = "enp5.tribalwars.net", DefaultTranslationId = 1 },
            new World { Id = 17, Name = "us41", Hostname = "us41.tribalwars.us", DefaultTranslationId = 1 },
            new World { Id = 18, Name = "en108", Hostname = "en108.tribalwars.net", DefaultTranslationId = 1 }
        };
    }
}
