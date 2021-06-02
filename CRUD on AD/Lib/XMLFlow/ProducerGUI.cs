using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Threading;
using RabbitMQ.Client;

namespace Lib.XMLFlow
{
    public class ProducerGUI
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


                bool xmlValidation = true;


                if (xmlValidation) //Change to XMLValidation
                {
                    Console.WriteLine("ProducerGUI: valid");




                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "direct_logs",
                                         routingKey: severity,
                                         basicProperties: null,
                                         body: body);


                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);

                    

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


