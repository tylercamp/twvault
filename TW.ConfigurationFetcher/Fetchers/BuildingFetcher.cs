using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TW.ConfigurationFetcher.Fetcher
{
    class BuildingFetcher : IFetcher
    {
        public override string Endpoint => "/interface.php?func=get_building_info";

        public override string Label => "buildings";

        public override void Process(String source, string fetchedContents)
        {
            var xml = ParseXml(fetchedContents);

            File.WriteAllText($"{source}_buildings.csv", new XmlToCsvWriter(xml)
                //.add
                .ToString());
        }
    }
}
