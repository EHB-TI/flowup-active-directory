using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Threading;
using RabbitMQ.Client;

namespace Lib.XMLFlow
{
    /**
    *  Class: RabbitMQ Producer without XML Validation 
    *         to send Messages on the GUI Queue;
    */
    public class ProducerGUI
    {
        public static bool send(string message, string severity)
        {
            //Initialize new connection to the RabbitMQ server
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Declare Exchhange
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");


                bool xmlValidation = true;


                if (xmlValidation) //Change to XMLValidation
                {
                    Console.WriteLine("ProducerGUI: valid");


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
                    Console.WriteLine("Producer: not valid");
                }
            }
            return false;
        }
    }
}


