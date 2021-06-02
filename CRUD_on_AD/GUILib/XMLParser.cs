using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Lib
{
    public static class XMLParser
    {
        public static string ReadXMLTag(string xml, string tag)
        {
            string operation = "";
            try
            {
                var schema = new XmlSchemaSet();
                var xmlDoc = XDocument.Parse(xml);
                var row = xmlDoc.Descendants().Where(x => x.Name.LocalName == "user").First();
                operation = (string)GetSubElementValue(row, tag);
                Console.WriteLine(operation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(ReadXMLTag(xml, "error"));
            }
            return operation;
        }
        public static object GetSubElementValue(XElement container, string subElementName)
        {
            var subElement = container.Descendants().FirstOrDefault(d => d.Name.LocalName == subElementName);
            if (subElement == null) return null;
            return subElement.Value;
        }
        public static void RemoveXMLTag(string xml, string tag)
        {
            var xmlDoc = XDocument.Parse(xml);
            xmlDoc.Descendants(tag).First().Remove();           
        }
        public static string ObjectToXML<T>(T user)
        {
            var writer = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(writer, user);
            return writer.ToString();
        }
        public static T XMLToObject<T>(string xml)
        {
            return (T) new XmlSerializer(typeof(T)).Deserialize(new StringReader(xml));
        }
    }
}
