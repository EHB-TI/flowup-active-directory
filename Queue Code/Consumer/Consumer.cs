using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Consumer
{
    //-- HANDLEIDING --- APP RUNNEN---------------------------------------------------------------------------------------
    // Best EERST DE CONSUMER RUNNEN zodat deze de tijd heeft om te "subscriben".
    // Vervolgens kan je de Producer RUNNEN ( Je hebt hiervoor 2 VisualStudio Vensters voor nodig)
    // Als alles goed gaat wordt dan een bericht vanuit de Producer (via RabbitMQ) op een Queue geplaatst,
    // dat vervolgens door de Consumer wordt afgehaald(uitgelezen).

    /* 
    ----DOEL----------------------------------------------------------------------------
     In deze App is het de bedoeling om een Consumer "na te bouwen".
     Het doel is om message van de queue("demo-queue) te halen (RabbitMQ).

    ----DOCKER -------------------------------------------------------------------------
     We gebruiken hiervoor Docker, het volgende commendo zorgt ervoor dat je een rabbitMQ instantie kan aanmaken 
     met het management platform. (Om zo te testen of wat je doet wel werkt)

      =>  docker run -d --hostname localhost --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:3.8-management 

        * Als je dit commando gebruikt zal RabbitMQ standaart een User:"guest" met Wachtwoord:"guest" aanmaken.
        * Dit kan je teste met je browser: localhost:15672
     */
    static class Consumer
    {
        // Om een connectie te maken met RabbitMQ hebben we nood aan een package dat we kunnen toevoegen mbv. NuGet.
        // => Package "RabbitMQ.Client"


        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;

                //if (args.Length < 1)
                //{
                //    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                //                            Environment.GetCommandLineArgs()[0]);
                //    Console.WriteLine(" Press [enter] to exit.");
                //    Console.ReadLine();
                //    Environment.ExitCode = 1;
                //    return;

                //}

                //foreach (var severity in args)
                //{
                //    channel.QueueBind(queue: queueName,
                //                      exchange: "direct_logs",
                //                      routingKey: severity);
                //}
                
                
                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs",
                                  routingKey: "createuser");
                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey, message);
                    if(routingKey == "createuser")
                    {
                        Console.WriteLine("create user");
                        
                    }
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}