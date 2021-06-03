using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Threading;
using RabbitMQ.Client;

namespace Lib.XMLFlow
{
    /**
    *  Class: RabbitMQ Producer with XML Validation 
    *         to send Messages on other Queues;
    *         Mostly outside modules
    */
    public class ProducerV2
    {
        public static bool Send(string message, string severity)
        {
            //Initialize new connection to the RabbitMQ server
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Declare Exchhange
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

                //Initialize XML Validator
                XmlSchemaSet schema = new XmlSchemaSet();
                schema.Add("", "Userxsd.xsd");
                XDocument xml = XDocument.Parse(message, LoadOptions.SetLineInfo);

                bool xmlValidation = true;
                xml.Validate(schema, (sender, e) =>
                {
                    xmlValidation = false;
                });

                if (xmlValidation)
                {
                    Console.WriteLine("ProducerV2: valid");
                    //Encode XML string into bytes to send on the Queue
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "direct_logs",
                                         routingKey: severity,
                                         basicProperties: null,
                                         body: body);

                    //Confirmation that Message has been sent on the Queue
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
                    return true;
                }
                else
                {
                    Console.WriteLine("ProducerV2: not valid");
                }
            }
            return false;
        }
    }
}

