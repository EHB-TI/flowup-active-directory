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
    public class ConsumerGUI
    {
        public static string getMessag = "";
        public static CRUD CRUD { get; set; }
        //public static LogWriter Logger { get; set; }    

        public static EventingBasicConsumer getMessage()
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
                                  routingKey: Severity.GUI.ToString());
                Console.WriteLine(" [*] Waiting for messages.");
                //Logger.LogWrite("Waiting for messages on 'AD' Queue", typeof(ConsumerV2));

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    //Message
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Recieved '{0}':'{1}'", routingKey, message);
                    //Logger.LogWrite($"Received message on '{routingKey}' Queue; With message = {message}", typeof(ConsumerV2));
                    getMessag = message;

                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                //Console.ReadLine();
                return consumer;
            }
        }

    }
}
