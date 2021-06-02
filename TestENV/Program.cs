using System;
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

            Console.WriteLine("Initializing Producer...");
            Thread.Sleep(2000);

            var producer = new Producer();
            //Demo User Creation
            var demoUser = new User
            {
                UserData = new UserData
                {
                    FirstName = "Demo",
                    LastName = "User",
                    Email = "demo.user@desideriushogeschool.be", //Gegenereerd op basis Name
                    Role = "student",
                    Password = "Student1" //Gegenereerd
                },
                MetaData = new MetaData
                {
                    UUIDMaster = "333ade47-03d1-40bb-9912-9a6c86a60169",
                    Methode = CRUDMethode.CREATE,
                    Origin = "CANVAS",
                    TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")
                }
            };
            Console.ReadLine();

        }
    }
}
