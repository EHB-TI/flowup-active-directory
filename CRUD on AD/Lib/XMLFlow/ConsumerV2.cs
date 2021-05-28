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
    public class ConsumerV2
    {
        public static CRUD CRUD { get; set; }
        
        public static void getMessage()
        {
            CRUD = new CRUD();
            CRUD.Binding(Lib.Connection.LOCAL);

            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //rabbitMQ
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;


                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs",
                                  routingKey: Severity.AD.ToString());
                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    //Message
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;


                    //user xsd validation
                    XmlSchemaSet schema = new XmlSchemaSet();
                    schema.Add("", "Userxsd.xsd");
                    XDocument xml = XDocument.Parse(message, LoadOptions.SetLineInfo);
                    bool xmlValidation = true;

                    xml.Validate(schema, (sender, e) =>
                    {
                        xmlValidation = false;
                    });



                    if (xmlValidation) //Has to change
                    {
                        Console.WriteLine("valid");


                        //Get CRUD Operation and tranfser to functionality
                        if (XMLParser.ReadXMLTag(message, "origin") == "AD")
                        {
                            XMLParser.ReadXMLTag(message, "method").OperationToCRUD(XMLParser.XMLToIntraObject(message), CRUD);
                        }
                        else if (XMLParser.ReadXMLTag(message, "origin") == "UUID")
                        {
                            var user = XMLParser.XMLToExtraObject(message);
                            user.MetaData.Origin = "AD";
                            user.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");
                            ProducerV2.send(XMLParser.ExtraObjectToXML(user), Severity.user.ToString());
                        }
                    }
                    else
                    {
                        //xsd validation error
                        schema = new XmlSchemaSet();
                        schema.Add("", "Errorxsd.xsd");
                        xml = XDocument.Parse(message, LoadOptions.SetLineInfo);
                        xmlValidation = true;

                        xml.Validate(schema, (sender, e) =>
                        {
                            xmlValidation = false;
                        });


                        //xml parsen
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
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }
        private static string createUserXml(XDocument xml)
        {
            string method = "";
            string origin = "AD";
            string version = "";
            string sourceEntityId = "";
            string timestamp = "";

            string firstname = "";
            string lastname = "";
            string email = "";
            string birthday = "";
            string role = "";
            string study = "";



            IEnumerable<XElement> xElements = xml.Descendants("method");
            foreach (var element in xElements)
            {
                method = element.Value;
            }
            xElements = xml.Descendants("version");
            foreach (var element in xElements)
            {
                version = element.Value;
            }
            xElements = xml.Descendants("sourceEntityId");
            foreach (var element in xElements)
            {
                sourceEntityId = element.Value;
            }
            xElements = xml.Descendants("timestamp");
            foreach (var element in xElements)
            {
                timestamp = element.Value;
            }
            xElements = xml.Descendants("firstname");
            foreach (var element in xElements)
            {
                firstname = element.Value;
            }
            xElements = xml.Descendants("lastname");
            foreach (var element in xElements)
            {
                lastname = element.Value;
            }
            xElements = xml.Descendants("email");
            foreach (var element in xElements)
            {
                email = element.Value;
            }
            xElements = xml.Descendants("birthday");
            foreach (var element in xElements)
            {
                birthday = element.Value;
            }
            xElements = xml.Descendants("role");
            foreach (var element in xElements)
            {
                role = element.Value;
            }
            xElements = xml.Descendants("study");
            foreach (var element in xElements)
            {
                study = element.Value;
            }
            string message = "";
            xElements = xml.Descendants("UUID");
            foreach (var element in xElements)
            {
                message = "<user><header>" +
                    "<UUID>" + element.Value + "</UUID>" +
                    "<method>" + method + "</method>" +
                    "<origin>" + origin + "</origin>" +
                    "<version>" + version + "</version>" +
                    "<sourceEntityId>" + sourceEntityId + "</sourceEntityId>" +
                    "<timestamp>" + timestamp + "</timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>" + firstname + "</firstname>" +
                    "<lastname>" + lastname + "</lastname>" +
                    "<email>" + email + "</email>" +
                    "<birthday>" + birthday + "</birthday>" +
                    "<role>" + role + "</role>" +
                    "<study>" + study + "</study>" +
                    "</body></user >";
            }

            return message;
        }
    }
}

