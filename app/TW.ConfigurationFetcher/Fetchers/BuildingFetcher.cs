using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher.Fetcher
{
    class BuildingFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_building_info";

        public override string Label => "buildings";

        public override bool NeedsUpdate(World world) => false;

        public override void Process(VaultContext context, World world, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);
        }
    }
}
