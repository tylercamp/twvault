using System;
using System.Collections.Generic;
using System.Text;

namespace TW.ConfigurationFetcher.Fetcher
{
    class UnitFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_unit_info";

        public override string Label => "units";

        public override void Process(String source, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);
        }
    }
}
