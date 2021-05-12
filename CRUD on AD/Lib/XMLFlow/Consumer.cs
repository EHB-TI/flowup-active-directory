using Lib.UUIDFlow;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib
{
     public class Consumer
    {
        public CRUD CRUD { get; set; }
        public UUIDConnection UConnection { get; set; }

        public Consumer()
        {
            CRUD = new CRUD();
            CRUD.Binding(Lib.Connection.LOCAL);

            UConnection = new UUIDConnection();
        }
        public void ConsumeMessage()
        {
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs",
                                  routingKey: "user");
                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey, message);

                    //Get CRUD Operation and tranfser to functionality
                    XMLParser.ReadXMLOperation(message).OperationToCRUD(XMLParser.XMLToObject(message), CRUD, UConnection);

                    //if (routingKey == "user")
                    //{
                    //    Console.WriteLine("user");
                    //}
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                Console.ReadLine();
            }
        }
    }
}
