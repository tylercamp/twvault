using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace TW.ConfigurationFetcher
{
    class CsvWriter
    {
        List<string> columnNames = new List<string>();
        List<List<string>> rows = new List<List<string>>();

        public CsvWriter AddSimpleColumn(string columnName, string value)
        {
            columnNames.Add(columnName);

            if (rows.Count == 0)
                rows.Add(new List<string>());

            rows[0].Add(value);

            return this;
        }

        public CsvWriter SetHeaders(params string[] headers)
        {
            this.columnNames = headers.ToList();
            return this;
        }

        public CsvWriter AddRow(params string[] values)
        {
            this.rows.Add(values.ToList());
            return this;
        }

        public CsvWriter AddRow(Action<List<string>> generator)
        {
            var row = new List<String>();
            this.rows.Add(row);
            generator(row);
            return this;
        }

        public CsvWriter AddEmpty(int count = 1)
        {
            for (int i = 0; i < count; i++)
                this.rows.Add(new List<string>());
            return this;
        }

        public override string ToString()
        {
            var numColumns = new int[] { columnNames.Count }.Concat(rows.Select(r => r.Count)).Max();

            var effectiveRows = new[] { new List<string>(columnNames) }.Concat(rows.Select(r => new List<string>(r))).ToList();
            foreach (var row in effectiveRows)
            {
                while (row.Count < numColumns)
                    row.Add("");
            }

            return string.Join('\n', effectiveRows.Select(r => string.Join(',', r)));
        }
    }

    class XmlToCsvWriter
    {
        CsvWriter writer = new CsvWriter();
        XPathNavigator xpath;
        public XmlToCsvWriter(XPathNavigator xpath)
        {
            this.xpath = xpath;
        }

        public XmlToCsvWriter SetHeaders(params string[] headers)
        {
            writer.SetHeaders(headers);
            return this;
        }

        public XmlToCsvWriter AddRowByTemplate(string basePath, params string[] subPaths)
        {
            writer.AddRow((row) =>
            {
                foreach (var path in subPaths)
                {
                    var fullPath = Path.Join(basePath, path);
                    if (!fullPath.StartsWith('/')) fullPath = '/' + fullPath;


                }
            });

            return this;
        }

        public XmlToCsvWriter AddDirectSimpleColumn(string columnName, object directValue)
        {
            writer.AddSimpleColumn(columnName, directValue.ToString());
            return this;
        }

        public XmlToCsvWriter AddSimpleColumn(string columnName, string path)
        {
            writer.AddSimpleColumn(columnName, xpath.SelectSingleNode(path).Value);
            return this;
        }

        public XmlToCsvWriter AddSimpleColumn<T>(string columnName, string path)
        {
            var value = xpath.SelectSingleNode(path).ValueAs(typeof(T));
            writer.AddSimpleColumn(columnName, value.ToString());
            return this;
        }

        public XmlToCsvWriter AddSimpleColumn(string columnName, string path, Func<string, object> selector)
        {
            var value = xpath.SelectSingleNode(path).Value;
            var selected = selector(value);
            writer.AddSimpleColumn(columnName, selected.ToString());
            return this;
        }

        public XmlToCsvWriter AddSimpleColumn<T>(string columnName, string path, Func<T, object> selector)
        {
            var value = (T)xpath.SelectSingleNode(path).ValueAs(typeof(T));
            var selected = selector(value);
            writer.AddSimpleColumn(columnName, selected.ToString());
            return this;
        }

        public override string ToString() => writer.ToString();
    }
}
