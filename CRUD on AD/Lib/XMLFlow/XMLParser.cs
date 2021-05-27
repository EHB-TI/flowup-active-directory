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
using Lib.UUIDFlow;
using Lib.XMLFlow;

namespace Lib
{
    public static class XMLParser
    {
        public static void OperationToCRUD(this string operation, User user, CRUD crudInstance) 
        {
            Debug.WriteLine("OperationTOcrud");
            bool succes;
            var adUser = user.UserObjectToADObject();
            switch (operation.ToUpperInvariant())
            {
                case "CREATE":
                    succes = crudInstance.CreateUser(user.UserObjectToADObject());
                    if (crudInstance.IsUserInAD(adUser.CN) && user.MetaData.GUID == "Not Set")
                    {
                        user.MetaData.GUID = crudInstance.FindADUser(adUser.CN).ObjectGUID;
                    }
                    break;
                case "DELETE":
                    succes = crudInstance.DeleteUser(adUser.CN);
                    break;
                case "UPDATE":
                    var oldUser = crudInstance.FindADUser("objectGUID="+user.MetaData.GUID);//Get GUID -> Search AD with GUID -> Convert DirectoryEntry to ADUserObject
                    succes = crudInstance.UpdateUser(oldUser, user.UserObjectToADObject());
                    break;
                //case "READ":
                //    break;
                //case "NOT SET":
                //    break;
                default:
                    Console.WriteLine(operation);
                    succes = false;
                    break;
            }
            if (succes)
            {
                Uuid.Update(ObjectToXML(user));
                /*
                 <?xml version="1.0" encoding="utf-16"?>
                <user xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <header>
                    <method>CREATE</method>
                    <origin>AD</origin>
                    <version>1</version>
                    <sourceEntityId>NOT SET</sourceEntityId>
                    <timestamp>2021-05-27T15:16:40+02:00</timestamp>
                  </header>
                  <body>
                    <firstname>Test</firstname>
                    <lastname>create</lastname>
                    <password>Student1</password>
                    <email>test.create@student.dhs.be</email>
                    <birthday>2000-12-31</birthday>
                    <role>student</role>
                    <study>DigX</study>
                  </body>
                </user>'
                */
            }
        }
        public static string ReadXMLTag(string xml, string tag)
        {
            var schema = new XmlSchemaSet();
            var xmlDoc = XDocument.Parse(xml);
            var row = xmlDoc.Descendants().Where(x => x.Name.LocalName == "user").First();
            var operation = GetSubElementValue(row, tag);
            Console.WriteLine(operation);

            return (string)operation;
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

        public static string ObjectToXML(User user, bool hide_pass=true)
        { 
            var serializer = new XmlSerializer(typeof(User));
            var writer = new StringWriter();

            serializer.Serialize(writer, user);
            Debug.WriteLine(writer.ToString());

            if (hide_pass)
            {
                RemoveXMLTag(writer.ToString(), "password");
            }

            return writer.ToString();
        }
    }
}
