using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace TW.ConfigurationFetcher.Fetcher
{
    abstract class IFetcher
    {
        public abstract String Endpoint { get; }
        public abstract String Label { get; }
        public abstract void Process(String source, String fetchedContents);


        protected object IntToBool(int value) => value == 0 ? false : true;

        protected XPathNavigator ParseXml(String xml)
        {
            var readerSettings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };

            var doc = new XPathDocument(XmlReader.Create(new StringReader(xml), readerSettings));
            return doc.CreateNavigator();
        }
    }
}
