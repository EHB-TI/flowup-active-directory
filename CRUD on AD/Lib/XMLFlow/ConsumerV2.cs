using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using Lib.UserFlow;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lib.XMLFlow
{
    /**
    *  Class: RabbitMQ Consumer with XML Validation 
    *         to recieve Messages from other Queues;
    *         Currently only from the GUI and UUID
    */
    public class ConsumerV2
    {
        public static CRUD CRUD { get; set; }

        public static void getMessage()
        {
            //Initialize instance to perform CRUD operation on the Active Directory DB
            CRUD = new CRUD();
            CRUD.Binding();

            //Initialize new connection to the RabbitMQ server
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Declare Exchhange
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;

                //Bind to queue where to consume data from
                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs",
                                  routingKey: Severity.AD.ToString());
                Console.WriteLine(" [*] Waiting for messages.");

                //Declare consumer delegate to execute custom code on each event/message recieved
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    //Convert message from bytes into a XML string
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Recieved '{0}':'{1}'", routingKey, message);

                    XmlSchemaSet schema = new XmlSchemaSet();
                    schema.Add("", "Userxsd.xsd");
                    XDocument xml = XDocument.Parse(message, LoadOptions.SetLineInfo);


                    bool xmlValidation = true;
                    //If XML from the GUI, skip XML Validation as some XML formats are not valid and will throw errors
                    if (XMLParser.ReadXMLTag(message, "origin") != "GUI")
                    {
                        xml.Validate(schema, (sender, e) =>
                        {
                            xmlValidation = false;
                        });
                    }

                    //Check if validation is true
                    if (xmlValidation)
                    {
                        Console.WriteLine("Consumer: valid");
                       
                        //Check if the XML comes from the GUI or the UUID
                        if (XMLParser.ReadXMLTag(message, "origin") == "GUI")
                        {
                            var oper = XMLParser.ReadXMLTag(message, "method");
                            //Method that only comes from the GUI, it asks for more info on the user with only the Container name
                            if (oper.Equals("READ"))
                            {
                                //Only 2 options, UPDATE or DELETE
                                if (XMLParser.ReadXMLTag(message, "goal").Equals("UPDATE"))
                                {
                                    //Return User data to GUI to able to display and change the data
                                    if (!ProducerGUI.send(XMLParser.ObjectToXML(CRUD.FindADUser(XMLParser.ReadXMLTag(message, "cn")).ADObjectToIntraUserObject()), Severity.GUI.ToString()))
                                    {
                                        Console.WriteLine("##################################################");
                                        Console.WriteLine($"#    Producing Message on Queue has FAILED       #");
                                        Console.WriteLine("##################################################");
                                    }
                                }
                                else 
                                {
                                    "DELETE".OperationToCRUD(CRUD.FindADUser(XMLParser.ReadXMLTag(message, "cn")).ADObjectToIntraUserObject(), CRUD);
                                }
                            }
                            else
                            {
                                oper.OperationToCRUD(XMLParser.XMLToObject<IntraUser>(message), CRUD);
                            }
                        }
                        else if (XMLParser.ReadXMLTag(message, "origin") == "UUID")
                        {
                            /**
                             *  Both DELETE and UPDATE/CREATE have different formats XML that gets returned from the UUID
                             *  This poses a problem when converting to different user objects.
                             *  To be safe and alsow the problem with empty tags dissapearing, 
                             *  the outgoing XML is HARDCODED with dynamic user data
                             */
                            if (XMLParser.ReadXMLTag(message, "method") != "DELETE")
                            { 
                                var user = XMLParser.XMLToObject<ExtraUser>(message);

                                //Replace attributes to send a new message on the Queue
                                user.MetaData.Origin = "AD";
                                user.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");

                                //<?xml version="1.0" encoding="utf-8"?>
                                string xmlmessage = "<user><header>" +
                                "<UUID>" + user.MetaData.UUIDMaster + "</UUID>" +
                                "<method>" + user.MetaData.Methode + "</method>" +
                                "<origin>" + user.MetaData.Origin + "</origin>" +
                                "<version>" + user.MetaData.Version + "</version>" +
                                "<sourceEntityId></sourceEntityId>" +
                                "<timestamp>" + user.MetaData.TimeStamp + "</timestamp>" +
                                "</header>" +
                                "<body>" +
                                "<firstname>" + user.UserData.FirstName + "</firstname>" +
                                "<lastname>" + user.UserData.LastName + "</lastname>" +
                                "<email>" + user.UserData.Email + "</email>" +
                                "<birthday>" + user.UserData.BirthDay + "</birthday>" +
                                "<role>" + user.UserData.Role + "</role>" +
                                "<study>" + user.UserData.Study + "</study>" +
                                "</body></user>";

                                //Produce a message on the User queue
                                if (!ProducerV2.Send(xmlmessage, Severity.user.ToString()))
                                {
                                    Console.WriteLine("##################################################");
                                    Console.WriteLine($"# Producing Message on the User Queue has FAILED #");
                                    Console.WriteLine("##################################################");
                                }
                            }
                            else
                            {
                                var user = XMLParser.XMLToObject<UUIDUser>(message);

                                //Replace attributes to send a new message on the Queue
                                user.MetaData.Origin = "AD";
                                user.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");

                                //<?xml version="1.0" encoding="utf-8"?>
                                string xmlmessage = "<user><header>" +
                                "<UUID>" + user.MetaData.UUIDMaster + "</UUID>" +
                                "<method>" + user.MetaData.Methode + "</method>" +
                                "<origin>" + user.MetaData.Origin + "</origin>" +
                                "<version></version>" +
                                "<sourceEntityId></sourceEntityId>" +
                                "<timestamp>" + user.MetaData.TimeStamp + "</timestamp>" +
                                "</header>" +
                                "<body>" +
                                "<firstname>" + user.UserData.FirstName + "</firstname>" +
                                "<lastname>" + user.UserData.LastName + "</lastname>" +
                                "<email>" + user.UserData.Email + "</email>" +
                                "<birthday>" + user.UserData.BirthDay + "</birthday>" +
                                "<role>" + user.UserData.Role + "</role>" +
                                "<study>" + user.UserData.Study + "</study>" +
                                "</body></user>";

                                //Produce a message on the User queue
                                if (!ProducerV2.Send(xmlmessage, Severity.user.ToString()))
                                {
                                    Console.WriteLine("##################################################");
                                    Console.WriteLine($"# Producing Message on the User Queue has FAILED #");
                                    Console.WriteLine("##################################################");
                                }
                            }
                        }
                        else
                        {
                            //If the origin of the XML string is not known or not handled, than an alert will be fired
                            Console.WriteLine("##################################################");
                            Console.WriteLine($"#  ALERT: an XML String comes from the outside   #");
                            Console.WriteLine($"#         and has NOT been handled!              #");
                            Console.WriteLine("##################################################");
                        }
                    }
                    else
                    {
                        //When the XML string is not valid, another check will be taken place
                        Console.WriteLine("Consumer: not valid");
                        //xsd validation error
                        schema = new XmlSchemaSet();
                        schema.Add("", "Errorxsd.xsd");
                        xml = XDocument.Parse(message, LoadOptions.SetLineInfo);
                        xmlValidation = true;
                        
                        //Validating XML with new XSD Scheme
                        xml.Validate(schema, (sender, e) =>
                        {
                            xmlValidation = false;
                        });

                        //If valid, a log will be send on the Queue
                        if (xmlValidation)
                        {
                            string error = "<error>"+
                                            "<header>"+
                                            "<code>2000</code>"+
                                            "<origin>AD</origin>"+
                                            "<timestamp>"+DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")+"</timestamp>"+
                                            "</header>"+
                                            "<body>"+
                                            "<objectUUID></objectUUID>"+
                                            "<objectSourceId></objectSourceId>"+
                                            "<objectOrigin>AD</objectOrigin>"+
                                            "<description>Something went wrong when adding to the UUID master, DB error:</description>" +
                                            "</body>" +
                                            "</error>";
                            ProducerGUI.send(error, Severity.logging.ToString());
                        }
                        //If not, an error will be displayed (Same principal as the error catch from the ReadXMLTag Method inside the XMLParser.class)
                        XDocument xmlUser = XDocument.Parse(message);
                        string errorDescription = "";
                        IEnumerable<XElement> xElements = xmlUser.Descendants("description");
                        foreach (var element in xElements)
                        {
                            errorDescription = element.Value;
                        }
                        Console.WriteLine(errorDescription);
                    }
                };
                //Declare consumer delegate to the channel so some code gets executed when a new message is recieved
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                //Readline to suspend program to receieve messages, if any key is entered, the Consumer will Exit(0)
                Console.ReadLine();
            }
        }
    }
}

