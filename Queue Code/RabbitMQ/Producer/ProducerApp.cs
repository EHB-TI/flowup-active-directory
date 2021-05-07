using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Producer
{
   
   // Best EERST DE CONSUMER RUNNEN => zodat deze de tijd heeft om te "subscriben"
   
    static class ProducerApp
    {
        static void Main(string[] args)
        {
            //---CONNECTIE MAKEN MET RABBITMQ-------------------------------------------------------------------------
            // Om een connectie te kunnen maken met RabbitMQ gebruiken we een  "ConnectionFactory".
            // We maken een niewe ConnectionFactory aan waaraan we een URI meegeven.
            // ! Hiervoor moet je de RabbitMQ.Client namespace toevoegen:  " using RabbitMQ.Client"

            var factory = new ConnectionFactory
            {
                // De Uri volgt de AMQP pattern,
                // je geeft de gebruikersnaam gevolgd door ":" wachtwoord gevolgd doorr "@" naam van de host gevolgd door ":"  poortnummer
                Uri = new Uri("ampqp://guest:guest@localhost:5672")
            };
            // Nu kunnen we de ConnectionFactory gebruiken om een connectie aan te maken.
            // We gebruiken hiervoor de default "CreateConnection"  => retruns an IConnection Object!
            using var connection = factory.CreateConnection();

            //---CHANNEL AANMAKEN--------------------------------------------------------------------------------------
            // Nu we een verbinding hebben met RabbitMQ, moeten we nog een Channel aanmaken.
            // We gebruiken hiervoor de default"CreateModel" => returns an IModel (returns the channel created)
            using var channel = connection.CreateModel();

            //---QUEUE DECLAREREN---------------------------------------------------------------------------------------
            // Nu we een channel hebben kunnen we onze Queue declareren.
            // We gebruiken hiervoor de onze juist aangemaakte Channel(IModel) en roepen "QueueDeclare" hierop.
            // * We kunnen een Queue-naam meegeven,
            // * durable op true zetten(zodat de message op de Queue blijft zolang deze niet door een Consumer wordt afgehaald),
            // * de andere properties zet je best nu op false, en agruments geef je niets mee(null).(Deze zijn enkel belangrijk voor de Exchanges!)

            channel.QueueDeclare("demo-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            //---BERICHT PLAATSEN OP DE QUEUE----------------------------------------------------------------------------
            // We maken een bericht aan, die we dan zullen plaatsen op de Queue die we juist hebben aangemaakt.
            // Omdat dit enkel een demo is zullen we voor het bericht een anoniem Object aanmaken met: "naam" en "bericht"
            var message = new { Name = "Producer", Message = "Helllo!" };
            // Als we dit bericht nu willen verzenden moeten we dit eerste omvormen => Byte-Array
            // We gebruiken "JSONConvert"(package: Newtonsoft.Json) om het bericht te serialiseren en vervolgens om te zetten naar een Array van Bytes
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


            // Om het bericht nu te verzenden gebruiken de "BasicPublish",
            // * deze verwacht normaalgezien een Exchange-naam vermits we er geen hebben laten we deze leeg(""),
            // * vervolgens een Routing Key (kort gezegd de naam van de Queue,voor meer info => Confluence),
            // * BasicProperties laten we leeg (null),
            // * als laatste geven ons bericht mee (in Byte-Array)
            channel.BasicPublish("", "demo-queue", null, body);
        }
    }
}
