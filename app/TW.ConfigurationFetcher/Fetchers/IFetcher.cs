using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using TW.Vault.Scaffold;

namespace TW.ConfigurationFetcher.Fetcher
{
    abstract class IFetcher
    {
        public abstract String Endpoint { get; }
        public abstract String Label { get; }
        public abstract void Process(VaultContext context, World world, String fetchedContents);

        protected XmlParser ParseXml(String xml)
        {
            var readerSettings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            var doc = new XPathDocument(XmlReader.Create(new StringReader(xml), readerSettings));
            return new XmlParser(doc.CreateNavigator());
        }
    }
}
