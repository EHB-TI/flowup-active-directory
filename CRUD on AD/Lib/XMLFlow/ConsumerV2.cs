﻿using System;
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
    public class ConsumerV2
    {
        public static CRUD CRUD { get; set; }
        //public static LogWriter Logger { get; set; }    

        public static void getMessage()
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
                                  routingKey: Severity.AD.ToString());
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

                    XmlSchemaSet schema = new XmlSchemaSet();
                    schema.Add("", "Userxsd.xsd");
                    XDocument xml = XDocument.Parse(message, LoadOptions.SetLineInfo);


                    bool xmlValidation = true;

                    string origin = XMLParser.ReadXMLTag(message, "origin");
                    
                    if (origin != "AD" && origin != "GUI")
                    {


                        xml.Validate(schema, (sender, e) =>
                        {
                            xmlValidation = false;
                        });
                    }
                    



                    if (xmlValidation) //Has to change
                    {
                        Console.WriteLine("Consumer: valid");
                       
                        //Get CRUD Operation and tranfser to functionality
                        if (XMLParser.ReadXMLTag(message, "origin") == "GUI")
                        {
                            string oper = XMLParser.ReadXMLTag(message, "method");
                            if (oper.Equals("READ"))
                            {

                                XMLParser.ReadXMLTag(message, "goal").OperationToCRUD(CRUD.FindADUser(XMLParser.ReadXMLTag(message, "cn")).ADObjectToIntraUserObject(), CRUD);
                            }
                            else
                            {
                                oper.OperationToCRUD(XMLParser.XMLToObject<IntraUser>(message), CRUD);
                            }
                        }
                        else if (XMLParser.ReadXMLTag(message, "origin") == "UUID")
                        {
                            if (XMLParser.ReadXMLTag(message, "method") != "DELETE")
                            {
                                var user = XMLParser.XMLToExtraObject(message);

                                user.MetaData.Origin = "AD";
                                user.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");

                                //<?xml version="1.0" encoding="utf-8"?>
                                string xmlmessage = "<user><header>" +
                                "<UUID>" + user.MetaData.UUIDMaster + "</UUID>" +
                                "<method>" + user.MetaData.Methode + "</method>" +
                                "<origin>" + user.MetaData.Origin + "</origin>" +
                                "<version>" + user.MetaData.Version + "</version>" +
                                "<sourceEntityId></sourceEntityId>" +
                                "<timestamp>" + user.MetaData.TimeStamp + "</timestamp>" +
                                "</header>" +
                                "<body>" +
                                "<firstname>" + user.UserData.FirstName + "</firstname>" +
                                "<lastname>" + user.UserData.LastName + "</lastname>" +
                                "<email>" + user.UserData.Email + "</email>" +
                                "<birthday>" + user.UserData.BirthDay + "</birthday>" +
                                "<role>" + user.UserData.Role + "</role>" +
                                "<study>" + user.UserData.Study + "</study>" +
                                "</body></user>";



                                ProducerV2.send(xmlmessage, Severity.user.ToString());
                            }
                            else
                            {
                                var user = XMLParser.XMLToUUIDObject(message);

                                user.MetaData.Origin = "AD";
                                user.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");

                                //<?xml version="1.0" encoding="utf-8"?>
                                string xmlmessage = "<user><header>" +
                                "<UUID>" + user.MetaData.UUIDMaster + "</UUID>" +
                                "<method>" + user.MetaData.Methode + "</method>" +
                                "<origin>" + user.MetaData.Origin + "</origin>" +
                                "<version></version>" +
                                "<sourceEntityId></sourceEntityId>" +
                                "<timestamp>" + user.MetaData.TimeStamp + "</timestamp>" +
                                "</header>" +
                                "<body>" +
                                "<firstname>" + user.UserData.FirstName + "</firstname>" +
                                "<lastname>" + user.UserData.LastName + "</lastname>" +
                                "<email>" + user.UserData.Email + "</email>" +
                                "<birthday>" + user.UserData.BirthDay + "</birthday>" +
                                "<role>" + user.UserData.Role + "</role>" +
                                "<study>" + user.UserData.Study + "</study>" +
                                "</body></user>";



                                ProducerV2.send(xmlmessage, Severity.user.ToString());
                            }
                            
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("Consumer: not valid");
                        //xsd validation error
                        schema = new XmlSchemaSet();
                        schema.Add("", "Errorxsd.xsd");
                        xml = XDocument.Parse(message, LoadOptions.SetLineInfo);
                        xmlValidation = true;

                        
                        xml.Validate(schema, (sender, e) =>
                        {
                            xmlValidation = false;
                        });


                        //xml parsen
                        XDocument xmlUser = XDocument.Parse(message);
                        string errorDescription = "";
                        IEnumerable<XElement> xElements = xmlUser.Descendants("description");
                        foreach (var element in xElements)
                        {
                            errorDescription = element.Value;
                        }


                        Console.WriteLine(errorDescription);
                    }
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }
        private void sendToUserQUEUE<T>(T user)
        {
            
            
        }
        private static string createUserXml(XDocument xml)
        {
            string method = "";
            string origin = "AD";
            string version = "";
            string sourceEntityId = "";
            string timestamp = "";

            string firstname = "";
            string lastname = "";
            string email = "";
            string birthday = "";
            string role = "";
            string study = "";



            IEnumerable<XElement> xElements = xml.Descendants("method");
            foreach (var element in xElements)
            {
                method = element.Value;
            }
            xElements = xml.Descendants("version");
            foreach (var element in xElements)
            {
                version = element.Value;
            }
            xElements = xml.Descendants("sourceEntityId");
            foreach (var element in xElements)
            {
                sourceEntityId = element.Value;
            }
            xElements = xml.Descendants("timestamp");
            foreach (var element in xElements)
            {
                timestamp = element.Value;
            }
            xElements = xml.Descendants("firstname");
            foreach (var element in xElements)
            {
                firstname = element.Value;
            }
            xElements = xml.Descendants("lastname");
            foreach (var element in xElements)
            {
                lastname = element.Value;
            }
            xElements = xml.Descendants("email");
            foreach (var element in xElements)
            {
                email = element.Value;
            }
            xElements = xml.Descendants("birthday");
            foreach (var element in xElements)
            {
                birthday = element.Value;
            }
            xElements = xml.Descendants("role");
            foreach (var element in xElements)
            {
                role = element.Value;
            }
            xElements = xml.Descendants("study");
            foreach (var element in xElements)
            {
                study = element.Value;
            }
            string message = "";
            xElements = xml.Descendants("UUID");
            foreach (var element in xElements)
            {
                message = "<user><header>" +
                    "<UUID>" + element.Value + "</UUID>" +
                    "<method>" + method + "</method>" +
                    "<origin>" + origin + "</origin>" +
                    "<version>" + version + "</version>" +
                    "<sourceEntityId>" + sourceEntityId + "</sourceEntityId>" +
                    "<timestamp>" + timestamp + "</timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>" + firstname + "</firstname>" +
                    "<lastname>" + lastname + "</lastname>" +
                    "<email>" + email + "</email>" +
                    "<birthday>" + birthday + "</birthday>" +
                    "<role>" + role + "</role>" +
                    "<study>" + study + "</study>" +
                    "</body></user >";
            }

            return message;
        }
    }
}

