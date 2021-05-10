﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Lib;
using MainWindow;

namespace TestENV
{
    class Program
    {
        static void Main(string[] args)
        {
            //var x = new XMLParser();
            //var xml = "<?xml version=\"1.0\" encoding=\"utf - 8\"?><user><body><firstname>FirstName</firstname><lastname>LastName</lastname><email>Email</email><role>Role</role></body><operation>CREATE</operation></user>";

            //x.ReadXMLOperation(xml);
            //Console.Read();

            Console.WriteLine("Initializing Consumer...");

            Task.Run(() => {
                var consumer = new Consumer();
                consumer.ConsumeMessage();
            }).Wait(TimeSpan.FromSeconds(5));

            var producer = new Producer();
            var xmlParser = new XMLParser();
            //Demo User Creation
            var demoUser = new User
            {
                UserData = new UserData
                {
                    FirstName = "Demo",
                    LastName = "User",
                    Email = "demo.user@desideriushogeschool.be",
                    Role = "student",
                    Password = "Student1"
                },
                MetaData = new MetaData
                {
                    UUIDMaster = "333ade47-03d1-40bb-9912-9a6c86a60169",
                    Methode = CRUDMethode.CREATE,
                    Origin = "CANVAS",
                    TimeStamp = DateTime.Now
                }
            };

            Console.WriteLine("Press [enter] to proceed.");
            Console.ReadLine();

            Console.WriteLine("===================User Creation================\n");
            Console.WriteLine(demoUser + "\n");
            Console.ReadLine();

            Console.WriteLine("=================Produce on the QUEUE==============\n");
            var xmlString = xmlParser.WriteXMLfromObject(demoUser);
            producer.CreateMessage(xmlString + "\n");

            Console.WriteLine("=================Consume from the QUEUE==============\n");
            //var xmlFromQueue = xmlParser.WriteXMLfromObject(demoUser);
            //Console.WriteLine(xmlFromQueue);
            Console.ReadLine();

            Environment.Exit(-1);

            Console.WriteLine("=================From XML to Object==============\n");
            var newUser = xmlParser.XMLtoObject(xmlString);
            Console.WriteLine(newUser + "\n");
            Console.ReadLine();

            Console.WriteLine("Starting Program...");
            var program = new CRUD();
            Console.WriteLine("Connecting to Active Directory...");
            program.Binding(Connection.LOCAL);
            Console.WriteLine("Adding Demo User in Active Directory...");
            program.CreateUser(newUser);
            Console.WriteLine("\nUser succesfully created in Active Directory...");
            Console.ReadLine();

            Console.WriteLine("\n\n===================Update Demo User from XML================\n");
            var xmlUpdate = xmlParser.ReadXMLFiletoString(@"C:\User\Administrator\source\repos\AnakinDelabelle\Demo_AD-DS_-_AD-LDS\TestENV\xmlData\UpdateDemo.xml");
            Console.WriteLine(xmlUpdate);
            Console.ReadLine();

            Console.WriteLine("Updating Demo User...");
            program.UpdateUser("CN=Demo User", xmlParser.XMLtoObject(xmlUpdate));
            Console.WriteLine("Finding Updated User in Active Directory...");
            var updatedUser = program.FindADUser("CN=UpdatedDemo User");

            Console.WriteLine("\n===================Updated User================\n");
            Console.WriteLine(updatedUser + "\n");
            Console.ReadLine();

            Console.WriteLine("Deleting UpdatedDemo User...");
            program.DeleteUser("CN=UpdatedDemo User");
            Console.ReadLine();
        }
    }
}
