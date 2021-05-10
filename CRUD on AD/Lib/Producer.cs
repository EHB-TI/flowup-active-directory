using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Producer
    {
        public void CreateMessage(string xml)
        {
            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");


                string severity = "user";
                
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
                    "<role>student</role>" +
                    "</body></user >";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent on Queue '{0}':'{1}'", severity, message);
            }
            Console.WriteLine(" Press [enter] to exit.\n\n");
            Console.ReadLine();

        }
    }
}
