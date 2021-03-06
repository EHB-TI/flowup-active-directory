using System;
using System.Collections.Generic;
using System.Windows;
using InputWindow;
using System.Diagnostics;
using System.Xml.Linq;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using RabbitMQ.Client;
using Lib;

namespace MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DemoWindow : Window
    {
        public DemoWindow()
        {
            InitializeComponent();

            //testdata
            string message = "<ArrayOfADUser>"+
                            "<ADUser>"+
                            "<cn>testusertest</cn>"+
                            "<displayName>testusertest</displayName>"+
                            "<name>testusertest</name>"+
                            "<givenName>testuser</givenName>"+
                            "<userPrincipalName>testuser@desideriushogeschool.be</userPrincipalName>"+
                            "<sn>test</sn>" +
                            "<sAMAccountName>testuser</sAMAccountName>"+
                            "<objectGuid>f3c45679-df6d-4621-8dc7-54e9c39c94ca</objectGuid>"+
                            "<birthday>NotSet</birthday>"+
                            "<objectVersion>-1</objectVersion>"+
                            "<study>NotSet</study>"+
                            "</ADUser>"+
                            "<ADUser>" +
                            "<cn>testusertest</cn>" +
                            "<displayName>testusertest</displayName>" +
                            "<name>testusertest</name>" +
                            "<givenName>testuser</givenName>" +
                            "<userPrincipalName>testuser@desideriushogeschool.be</userPrincipalName>" +
                            "<sn>test</sn>" +
                            "<sAMAccountName>testuser</sAMAccountName>" +
                            "<objectGuid>f3c45679-df6d-4621-8dc7-54e9c39c94ca</objectGuid>" +
                            "<birthday>NotSet</birthday>" +
                            "<objectVersion>-1</objectVersion>" +
                            "<study>NotSet</study>" +
                            "</ADUser>" +
                            "<ADUser>" +
                            "<cn>testusertest</cn>" +
                            "<displayName>testusertest</displayName>" +
                            "<name>testusertest</name>" +
                            "<givenName>testuser</givenName>" +
                            "<userPrincipalName>testuser@desideriushogeschool.be</userPrincipalName>" +
                            "<sn>test</sn>" +
                            "<sAMAccountName>testuser</sAMAccountName>" +
                            "<objectGuid>f3c45679-df6d-4621-8dc7-54e9c39c94ca</objectGuid>" +
                            "<birthday>NotSet</birthday>" +
                            "<objectVersion>-1</objectVersion>" +
                            "<study>NotSet</study>" +
                            "</ADUser>" +
                            "</ArrayOfADUser>";
            //getusers(message);
            new Thread(() =>
            {
                getMessage();

            }).Start();
           
            lblCurrent.Content = "Current Connection: LOCAL";      //LOCAL or LDAP
            btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = btnChangeConnection.IsEnabled = false;
        }

        private void GetAllUsersAction(object sender, RoutedEventArgs e)
        {
            fieldResults.Items.Clear();
            try
            {
                string xmlmessage = "<user><header>"+
                                "<UUID>Not Set</UUID>" +
                                "<method>READ_ALL</method>" +
                                "<origin>GUI</origin>" +
                                "<version>0</version>" +
                                "<sourceEntityId>Not Set</sourceEntityId>" +
                                "<timestamp>"+DateTime.Now.ToString()+"</timestamp>" +
                                "</header>" +
                                "<body>" +
                                "<firstname>Not Set</firstname>" +
                                "<lastname>Not Set</lastname>" +
                                "<email>Not Set</email>" +
                                "<birthday>Not Set</birthday>" +
                                "<role>Not Set</role>" +
                                "<study>Not Set</study>" +
                                "</body></user>";

                //Console.WriteLine(XMLParser.ObjectToXML(Program.GetADUsers()));
                ProducerGUI.send(xmlmessage, Severity.AD.ToString());

                

                
                
                //List<ADUser> l = null;
                /*
                if (l != null)
                {
                    l.ForEach(x => fieldResults.Items.Add($"CN={x.CN}"));
                }
                else
                {
                    MessageBox.Show("No User found or Database not connected!");
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = true;
        }

        private void CreateUserAction(object sender, RoutedEventArgs e)
        {
            DialogWindow w = new DialogWindow();
            w.ShowDialog();
            if (w.DialogResult == true)
            {
                try
                {
                    var user = w.Answer;
                    if (ProducerV2.send(XMLParser.ObjectToXML(user), Severity.AD.ToString()))
                    {
                        Console.WriteLine(XMLParser.ObjectToXML(user));
                        //XML Object Send over AD Queueu
                        /*
                        <?xml version="1.0" encoding="utf-16"?>
                        <user xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                          <header>
                            <method>CREATE</method>
                            <origin>AD</origin>
                            <version>1</version>
                            <sourceEntityId>339f133f-647f-4481-982a-4b0db8ee0c7e</sourceEntityId>
                            <timestamp>2021-05-28T11:40:16+02:00</timestamp>
                          </header>
                          <body>
                            <firstname>Test</firstname>
                            <lastname>Everything</lastname>
                            <email>test.everything@student.dhs.be</email>
                            <birthday>2000-07-05</birthday>
                            <role>student</role>
                            <study>Digx</study>
                          </body>
                        </user>'
                        */

                        MessageBox.Show("User send created!");
                        btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ChangeConnectionAction(object sender, RoutedEventArgs e)
        {
            btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
        }

        private void DeleteUserAction(object sender, RoutedEventArgs e)
        {
            if (fieldResults.SelectedIndex != -1)
            {
                Debug.WriteLine(fieldResults.SelectedValue.ToString());

                string xmlmessage = "<user>" +
                                $"<cn>{fieldResults.SelectedValue.ToString()}</cn>" +
                                "<method>READ</method>" +
                                "<goal>DELETE</goal>" +
                                "<origin>GUI</origin>" +
                                "<timestamp>" + DateTime.Now.ToString() + "</timestamp>" +
                                "</user>";

                if (ProducerGUI.send(xmlmessage, Severity.AD.ToString())
)               {
                    MessageBox.Show("User succesfully deleted!");
                    btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Select a user first!");
            }
        }
        public void DeleteUser(IntraUser user)
        {
            if (ProducerV2.send(XMLParser.ObjectToXML(user), Severity.AD.ToString()))
            {

                /*
                 <user xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <header>
                    <method>DELETE</method>
                    <origin>AD</origin>
                    <version>0</version>
                    <timestamp>2021-05-26T15:44:41+02:00</timestamp>
                  </header>
                  <body>
                    <firstname>test</firstname>
                    <lastname>d</lastname>
                    <email>test.d@desideriushogeschool.be</email>
                    <role>docent</role>
                  </body>
                </user>
                */
              
            }
        }

        private void UpdateUserAction(object sender, RoutedEventArgs e)
        {
            if (fieldResults.SelectedIndex != -1)
            {
                Console.WriteLine("Update User");
                MessageBox.Show("Update Action recieved; no implementation yet!!!");
            }
            else
            {
                MessageBox.Show("Select a user first!");
            }
        }

        public void UpdateUser(IntraUser oldUser)
        {
            DialogWindow w = new DialogWindow(oldUser);
            w.ShowDialog();

            if (w.DialogResult == true)
            {
                Debug.WriteLine(fieldResults.SelectedValue.ToString());
                //try
                //{
                var user = w.Answer;
                if (ProducerV2.send(XMLParser.ObjectToXML(user), Severity.AD.ToString()))
                {
                    Console.WriteLine(XMLParser.ObjectToXML(user));

                    /*
                     <?xml version="1.0" encoding="utf-16"?>
                    <user xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                        <header>
                            <method>UPDATE</method>
                            <origin>AD</origin>
                            <version>1</version>
                            <timestamp>2021-05-26T15:42:39+02:00</timestamp>
                        </header>
                        <body>
                            <firstname>test</firstname>
                            <lastname>up</lastname>
                            <email>test.up@student.dhs.be</email>
                            <role>docent</role>
                        </body>
                    </user>
                     */
                    MessageBox.Show("Updated user succesfully send!");
                    btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
                }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}
            }
        }

        public void GetUsers(string messange)
        {

            //XDocument xmlUser = XDocument.Parse(messange);
            var test = XMLParser.XMLToObject<List<ADUser>>(messange);
            foreach(var user in test)
            {
                fieldResults.Items.Add($"CN={user.CN}");
            }
        }

        public void getMessage()
        {

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
                    this.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var user = XMLParser.XMLToObject<IntraUser>(message);

                        } 
                        catch (Exception e)
                        {
                            GetUsers(message);
                        }
                       
                    });
                    

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
