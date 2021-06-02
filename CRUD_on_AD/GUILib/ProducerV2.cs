using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Threading;
using RabbitMQ.Client;

namespace Lib.XMLFlow
{
    public class ProducerV2
    {
        public static bool send(string message, string severity)
        {

            Thread.Sleep(2000);

            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");





                XmlSchemaSet schema = new XmlSchemaSet();

                schema.Add("", "Userxsd.xsd"); //Change to Relative Path

                XDocument xml = XDocument.Parse(message, LoadOptions.SetLineInfo);


                bool xmlValidation = true;

                string origin = XMLParser.ReadXMLTag(message, "origin");

                if (origin != "GUI")
                {


                    xml.Validate(schema, (sender, e) =>
                    {
                        xmlValidation = false;
                    });
                }

                if (xmlValidation) //Change to XMLValidation
                {
                    Console.WriteLine("Producer: valid");




                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "direct_logs",
                                         routingKey: severity,
                                         basicProperties: null,
                                         body: body);


                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);

                    XDocument doc = XDocument.Parse(message);

                    return true;
                }
                else
                {
                    Console.WriteLine("Producer: not valid");
                }
            }
            return false;
        }
    }
}

