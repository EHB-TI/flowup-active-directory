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
        public static void OperationToCRUD(this string operation, User user) 
        {
            switch (operation.ToUpperInvariant())
            {
                case "CREATE":
                    break;
                case "DELETE":
                    break;
                case "UPDATE":
                    break;
                case "READ":
                    break;
                case "NOT SET":
                    break;
                default:
                    Console.WriteLine(operation);
                    break;
            }
        }
        public static string ReadXMLOperation(string xml)
        {
            var schema = new XmlSchemaSet();
            var xmlDoc = XDocument.Parse(xml);
            var row = xmlDoc.Descendants().Where(x => x.Name.LocalName == "user").First();
            var operation = GetSubElementValue(row, "method");
            Console.WriteLine(operation);

            return (string)operation;
        }

        public static object GetSubElementValue(XElement container, string subElementName)
        {
            var subElement = container.Descendants().FirstOrDefault(d => d.Name.LocalName == subElementName);
            if (subElement == null) return null;
            return subElement.Value;
        }

        public static string ReadXMLFiletoString(string path)
        {
            XmlSerializer reader = new XmlSerializer(typeof(User));
            StreamReader file = new StreamReader(path);
            var xml = ObjectToXML((User)reader.Deserialize(file));
            file.Close();
            return xml;
        }
        public static User XMLToObject(string xml)
        {
            //if (!ValidateXML(xml))   
            //{
            //    Debug.WriteLine("Ongeldige XML!!!");
            //}

            var serializer = new XmlSerializer(typeof(User));
            var reader = new StringReader(xml);
            var user = (User)serializer.Deserialize(reader);

            return user;
        }
        public static bool ValidateXML(string xml)
        {
            var schema = new XmlSchemaSet();
            var xmlDoc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
            var check = true;

            schema.Add("", @"..\..\..\TestENV\xmlData\xsdcontrole.xsd"); //Can change

            xmlDoc.Validate(schema, (sender, e) =>
            {
                check = false;
            });

            return check;
        }

        public static string ObjectToXML(User user)
        {
            // First write something so that there is something to read ...  
            //var user = new User { UserData = new UserData { FirstName = "Anakin", LastName = "Delabelle", Email = "anakin.delabelle@student.ehb.be", Role = "student" } };
            var serializer = new XmlSerializer(typeof(User));
            var writer = new StringWriter();

            serializer.Serialize(writer, user);
            Debug.WriteLine(writer.ToString());

            return writer.ToString();
        }
    }
}
