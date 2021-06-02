using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Management;

namespace CPURAM
{
    class Program
    {

        //protected static PerformanceCounter cpuCounter;
        //protected static PerformanceCounter ramCounter;
        protected static double totalRam = 100000;

        static void Main(string[] args)
        {
            //Set CPU and RAM counter to sent w/ heartbeat
            //cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //ramCounter = new PerformanceCounter("Memory", "Available KBytes");
           

         
            


            var factory = new ConnectionFactory() { HostName = "10.3.56.6" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                while (true)
                {
                    DateTime dt = DateTime.Now;
                    //set message in xml
                    string xmlmessage = "<heartbeat><header><code>2000</code>" +
                    "<origin>AD</origin>" +
                    $"<timestamp>{dt.ToString("yyyy-MM-ddTHH:mm:ss%K")}</timestamp></header>" +
                    $"<body><nameService>ADFS</nameService><CPUload>{1}</CPUload>" +
                    $"<RAMload>{1}</RAMload></body>" +
                    "</heartbeat>";
                    string message = xmlmessage;


                    XmlSchemaSet schema = new XmlSchemaSet();

                    schema.Add("", "Heartbeat.xsd");

                    XDocument xml = XDocument.Parse(xmlmessage, LoadOptions.SetLineInfo);


                    bool xmlValidation = true;

                    xml.Validate(schema, (sender, e) =>
                    {
                        xmlValidation = false;
                    });



                    if (xmlValidation)
                    {
                        Console.WriteLine("XML is geldig");
                        Console.WriteLine(" [x] Sent {0}", message);

                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                             routingKey: "heartbeat",
                                             basicProperties: null,
                                             body: body);
                    }
                    else
                    {
                        Console.WriteLine("XML is ongeldig");
                    }


                    //stop the program for 1 second
                    Thread.Sleep(1000);

                }


                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

       /** static double getCpuUsage()
        {

            double cpu = cpuCounter.NextValue();
            cpu = cpu / 100;
            Console.WriteLine(cpu);


            return cpu;
        }

        static string getRamUsage()
        {


            double usageRam = ramCounter.NextValue();


            double avRam = (usageRam / totalRam );
            Console.WriteLine(avRam);

            return avRam.ToString().Replace(',', '.');
        }

        */
    }
}
