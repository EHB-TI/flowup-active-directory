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

namespace Lib.XMLFlow
{
    /**
    *  Class: A XML Parser to map functionality to user objects; 
    *         Alsow converting different object with XML and 
    *         reading values from XML strings
    */
    public static class XMLParser
    {
        /**
         *  Main Methode: Reads the operation and assigns the right CRUD operation on the incoming user
         *      @param1 => The method/operation of the CRUD to be executed
         *      @param2 => The incoming user object that is used for the CRUD
         *      @param3 => The CRUD instance with all the CRUD functionality inside    
         */
        public static void OperationToCRUD(this string operation, IntraUser user, CRUD crudInstance)
        {
            //Makes an AD/C# user object for ease of use 
            var adUser = user.IntraUserObjectToADObject();
            //Check for operation with switch case
            switch (operation.ToUpperInvariant())
            {
                //Creates a new user in the Active Directory DB
                case "CREATE":
                    if (crudInstance.CreateUser(user.IntraUserObjectToADObject()))
                    {
                        //Check if new user made it into the DB
                        if (crudInstance.IsUserInAD(adUser.CN))
                        {
                            //Get the newly generated GUID from the Attributes
                            user.MetaData.GUID = crudInstance.FindADUser(adUser.CN).ObjectGUID;
                            //Update the UUID with the newly created user
                            Uuid.Update(user);
                        }
                        else
                        {
                            //If user is not found, display an error
                            Console.WriteLine("##################################################");
                            Console.WriteLine($"#      Newly created user cannot be found        #");
                            Console.WriteLine("##################################################");
                        }
                    }
                    else
                    {
                        //Show error if Create got interrupted
                        Console.WriteLine("##################################################");
                        Console.WriteLine($"# CREATING user from Active Directory has FAILED #");
                        Console.WriteLine("##################################################");

                        //Make a XML Error string to log error
                        string error = "<error>" +
                                        "<header>" +
                                        "<code>3003</code>" +
                                        "<origin>AD</origin>" +
                                        "<timestamp>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") + "</timestamp>" +
                                        "</header>" +
                                        "<body>" +
                                        "<objectUUID></objectUUID>" +
                                        "<objectSourceId></objectSourceId>" +
                                        "<objectOrigin>AD</objectOrigin>" +
                                        "<description>The user could not be added to the AD</description>" +
                                        "</body>" +
                                        "</error>";
                        //Send the error on the Logging Queue
                        ProducerGUI.send(error, Severity.logging.ToString());
                    }
                    break;
                //Deletes an existing user from the Active Directory DB
                case "DELETE":
                    if (crudInstance.DeleteUser(adUser.CN))
                    {
                        /**
                         *  Because the GUI only has the Container name of all the users
                         *  the DELETE method only comes wrapped around a READ method, 
                         *  to first find the user and with that reference => delete the user
                         */
                        user.MetaData.Methode = CRUDMethode.DELETE;
                        //Update the UUID with the deleted user
                        Uuid.Update(user);
                    }
                    else
                    {
                        //Show error if Delete got interrupted
                        Console.WriteLine("##################################################");
                        Console.WriteLine($"# DELETING user from Active Directory has FAILED #");
                        Console.WriteLine("##################################################");
                    }
                    break;
                //Updates an existing user from the Active Directory DB
                case "UPDATE":
                    //Find the old user object based on the same GUID from both users
                    var oldUser = crudInstance.FindADUser("objectGUID=" + ConvertGuidToOctetString(user.MetaData.GUID));
                    if (crudInstance.UpdateUser(oldUser, user.IntraUserObjectToADObject()))
                    {
                        //Update the UUID with the newly updated user
                        Uuid.Update(user);
                    }
                    else
                    {
                        //Show error if Update got interrupted
                        Console.WriteLine("##################################################");
                        Console.WriteLine($"# UPDATING user from Active Directory has FAILED #");
                        Console.WriteLine("##################################################");
                    }
                    break;
                //Reads all users in Active Directory and wrap it into a List<ADUser>
                case "READ_ALL":
                    //Get all users
                    ListUsers.List = crudInstance.GetADUsers();
                    //Converts List into a XML Array and send it on the Queue of the GUI
                    ProducerGUI.send(ObjectToXML(ListUsers.List), Severity.GUI.ToString()); //Make new Producer
                    break;
                default:
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#       INVALID Operation: {operation} ");
                    Console.WriteLine("##################################################");
                    break;
            }
        }
        /**
         *  Methode: Convert the GUID object into a readable GUID string
         *      @param1 => The GUID object
         */
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
        /**
         *  Methode: Parse the XML string to get the value of a specific tag
         *      @param1 => The XML string that has to be parsed
         *      @param2 => The tag inside the XML node
         */
        public static string ReadXMLTag(string xml, string tag)
        {
            string operation = "";
            var schema = new XmlSchemaSet();
            var xmlDoc = XDocument.Parse(xml);
            try
            {
                var row = xmlDoc.Descendants().Where(x => x.Name.LocalName == "user").First();
                operation = (string)GetSubElementValue(row, tag);
                //Check to see the value is not null
                return operation is null
                    ? "Not Set"
                    : operation;
            }
            catch (Exception e1)
            {
                try
                {
                    //First catch: try reading the error when the xml-error comes from the UUID
                    var row = xmlDoc.Descendants().Where(x => x.Name.LocalName == "error").First();
                    operation = (string)GetSubElementValue(row, "description");
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#        UUID Error        ");
                    Console.WriteLine($"#           => {operation}        ");
                    Console.WriteLine("##################################################");
                }
                catch (Exception e2)
                {
                    //Second catch: try reading the error when the xml-error comes from the invalid syntax in the xml
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#        XML Error        #");
                    Console.WriteLine($"#           => {e1}        #");
                    Console.WriteLine($"#           => {e2}        #");
                    Console.WriteLine("##################################################");
                }
                return "ERROR";
            }
        }
        /**
         *  Methode: Get the value out of a XML tag
         *      @param1 => The XML row element where tag's are located
         *      @param2 => The tag to parse the value out
         */
        public static object GetSubElementValue(XElement container, string subElementName)
        {
            var subElement = container.Descendants().FirstOrDefault(d => d.Name.LocalName == subElementName);
            if (subElement == null) return null;
            return subElement.Value;
        }
        /**
         *  Methode: Convert an user object into a XML string
         *      @param1 => The user object to be converted
         */
        public static string ObjectToXML<T>(T user)
        {
            var writer = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(writer, user);
            return writer.ToString();
        }
        /**
         *  Methode: Convert a XML string into an user object
         *      @param1 => a XML string to be converted
         */
        public static T XMLToObject<T>(string xml)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(new StringReader(xml));
        }
    }
}
