using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace TW.ConfigurationFetcher
{
    class XmlParser
    {
        XPathNavigator xpath;

        public XmlParser(XPathNavigator xpath)
        {
            this.xpath = xpath;
        }

        private T ValueOrDefault<T>(XPathItem item) => item.Value == String.Empty ? default(T) : (T)item.ValueAs(typeof(T));

        public T Get<T>(string path) => ValueOrDefault<T>(xpath.SelectSingleNode(path));

        public R Get<T, R>(string path, Func<T, R> func) => func(ValueOrDefault<T>(xpath.SelectSingleNode(path)));

        public XmlParser Select(string path) => new XmlParser(xpath.SelectSingleNode(path));
    }
}
