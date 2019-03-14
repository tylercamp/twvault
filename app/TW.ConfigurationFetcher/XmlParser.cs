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

        public T Get<T>(string path) => (T)xpath.SelectSingleNode(path).ValueAs(typeof(T));

        public R Get<T, R>(string path, Func<T, R> func) => func((T)xpath.SelectSingleNode(path).ValueAs(typeof(T)));

        public XmlParser Select(string path) => new XmlParser(xpath.SelectSingleNode(path));
    }
}
