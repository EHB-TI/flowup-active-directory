using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
namespace RabbitMQ.Producer
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
    static class Producer
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

                /*    var severity = (args.Length > 0) ? args[0] : "info";
                    var message = (args.Length > 1)
                                  ? string.Join(" ", args.Skip(1).ToArray())
                                  : "Hello World!";*/
                string message = "<user><header>" +
                    "<UUID>333ade47-03d1-40bb-9912-9a6c86a60169</UUID>" +
                    "<method>CREATE</method>" +
                    "<origin>AD</origin>" +
                    "<timestamp> 2021 - 05 - 25T12: 00:00 + 01:00 </timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>Tibo</firstname>" +
                    "<lastname>De Munck</lastname>" +
                    "<email>tibo.de.munck@student.dhb.be</email>" +
                    "<role>student</role>"+
                    "</body></user >";
                string severity = "createuser";

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}