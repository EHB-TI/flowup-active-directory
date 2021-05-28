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
        public static void OperationToCRUD(this string operation, IntraUser user, CRUD crudInstance) 
        {
            Debug.WriteLine("OperationTOcrud");
            bool succes;
            var adUser = user.IntraUserObjectToADObject();
            switch (operation.ToUpperInvariant())
            {
                case "CREATE":
                    succes = crudInstance.CreateUser(user.IntraUserObjectToADObject());
                    if (crudInstance.IsUserInAD(adUser.CN))
                    {
                        Debug.WriteLine("Searching for GUID");
                        user.MetaData.GUID = crudInstance.FindADUser(adUser.CN).ObjectGUID;
                        Debug.WriteLine("GUID: "+user.MetaData.GUID);
                    }
                    else
                    {
                        throw new Exception("Newly created user can not be found!");
                    }
                    break;
                case "DELETE":
                    succes = crudInstance.DeleteUser(adUser.CN);
                    break;
                case "UPDATE":
                    Debug.WriteLine("Searching for old user with GUID: "+ user.MetaData.GUID);
                    var oldUser = crudInstance.FindADUser("objectGUID="+ConvertGuidToOctetString(user.MetaData.GUID));//Get GUID -> Search AD with GUID -> Convert DirectoryEntry to ADUserObject
                    succes = crudInstance.UpdateUser(oldUser, user.IntraUserObjectToADObject());
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
                ExtraUser outUser = user.ConvertIntraToExtra();
                outUser.MetaData = new MetaData
                {
                    GUID = user.MetaData.GUID,
                    Methode = user.MetaData.Methode,
                    Version = user.MetaData.Version,
                    Origin = "AD",
                    TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")
                };
                
                Uuid.Update(ExtraObjectToXML(outUser));
                /*
                 <?xml version="1.0" encoding="utf-16"?>
                    <user xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                      <header>
                        <method>CREATE</method>
                        <origin>AD</origin>
                        <version>1</version>
                        <sourceEntityId>NOT SET</sourceEntityId>
                        <timestamp>2021-05-28T10:56:48+02:00</timestamp>
                      </header>
                      <body>
                        <firstname>uuid</firstname>
                        <lastname>test</lastname>
                        <email>uuid.test@student.dhs.be</email>
                        <birthday>2000-12-07</birthday>
                        <role>student</role>
                        <study>digx</study>
                      </body>
                    </user>
                */
            }
        }
        internal static string ConvertGuidToOctetString(string objectGuid)
        {
            //From: https://stackoverflow.com/questions/1545630/searching-for-a-objectguid-in-ad
            System.Guid guid = new Guid(objectGuid);
            byte[] byteGuid = guid.ToByteArray();

            string queryGuid = "";

            foreach (byte b in byteGuid)
            {
                queryGuid += @"\" + b.ToString("x2");
            }

            return queryGuid;
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
            XmlSerializer reader = new XmlSerializer(typeof(IntraUser));
            StreamReader file = new StreamReader(path);
            var xml = IntraObjectToXML((IntraUser)reader.Deserialize(file));
            file.Close();
            return xml;
        }
        public static IntraUser XMLToIntraObject(string xml)
        {
            //if (!ValidateXML(xml))   
            //{
            //    Debug.WriteLine("Ongeldige XML!!!");
            //}

            var serializer = new XmlSerializer(typeof(IntraUser));
            var reader = new StringReader(xml);
            var user = (IntraUser)serializer.Deserialize(reader);

            return user;
        }
        public static ExtraUser XMLToExtraObject(string xml)
        {
            //if (!ValidateXML(xml))   
            //{
            //    Debug.WriteLine("Ongeldige XML!!!");
            //}

            var serializer = new XmlSerializer(typeof(ExtraUser));
            var reader = new StringReader(xml);
            var user = (ExtraUser)serializer.Deserialize(reader);

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

        public static string IntraObjectToXML(IntraUser user)
        { 
            var serializer = new XmlSerializer(typeof(IntraUser));
            var writer = new StringWriter();

            serializer.Serialize(writer, user);
            Debug.WriteLine(writer.ToString());

            return writer.ToString();
        }
        public static string ExtraObjectToXML(ExtraUser user)
        {
            var serializer = new XmlSerializer(typeof(ExtraUser));
            var writer = new StringWriter();

            serializer.Serialize(writer, user);
            Debug.WriteLine(writer.ToString());

            return writer.ToString();
        }
    }
}
