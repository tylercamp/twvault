using System;
using System.Collections.Generic;
using System.Text;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher.Fetcher
{
    class UnitFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_unit_info";

        public override string Label => "units";

        public override bool NeedsUpdate(World world) => false;

        public override void Process(VaultContext context, World world, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);
        }
    }
}
