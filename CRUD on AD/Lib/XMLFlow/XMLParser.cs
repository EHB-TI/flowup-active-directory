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
using Lib.UserFlow;
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
                    if(crudInstance.UpdateUser(oldUser, user.IntraUserObjectToADObject()))
                    {
                        Uuid.Update(user);
                    }
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
        public static IntraUser XMLToIntraObject(string xml)
        {
            var serializer = new XmlSerializer(typeof(IntraUser));
            var reader = new StringReader(xml);
            var user = (IntraUser)serializer.Deserialize(reader);
            return user;
        }
        public static ExtraUser XMLToExtraObject(string xml)
        {
            var serializer = new XmlSerializer(typeof(ExtraUser));
            var reader = new StringReader(xml);
            var user = (ExtraUser)serializer.Deserialize(reader);
            return user;
        }
        public static UUIDUser XMLToUUIDObject(string xml)
        {
            var serializer = new XmlSerializer(typeof(UUIDUser));
            var reader = new StringReader(xml);
            var user = (UUIDUser)serializer.Deserialize(reader);
            return user;
        }
        public static string IntraObjectToXML(IntraUser user)
        { 
            var serializer = new XmlSerializer(typeof(IntraUser));
            var writer = new StringWriter();
            serializer.Serialize(writer, user);
            return writer.ToString();
        }
        public static string ExtraObjectToXML(ExtraUser user)
        {
            var serializer = new XmlSerializer(typeof(ExtraUser));
            var writer = new StringWriter();
            serializer.Serialize(writer, user);
            return writer.ToString();
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
