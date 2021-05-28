using System;
using System.Collections.Generic;
using System.Windows;
using Lib;
using InputWindow;
using System.Diagnostics;
using Lib.XMLFlow;
using Lib.UserFlow;

namespace MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DemoWindow : Window
    {
        public CRUD Program { get; set; }

        public DemoWindow()
        {
            InitializeComponent();

            Program = new CRUD();
            Program.Binding(Connection.LOCAL);
            lblCurrent.Content = "Current Connection: LOCAL";      //LOCAL or LDAP
            btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = btnChangeConnection.IsEnabled = false;
        }

        private void GetAllUsersAction(object sender, RoutedEventArgs e)
        {
            fieldResults.Items.Clear();
            try
            {
                List<ADUser> l = Program.GetADUsers();
                if (l != null)
                {
                    l.ForEach(x => fieldResults.Items.Add($"CN={x.CN}"));
                }
                else
                {
                    MessageBox.Show("No User found or Database not connected!");
                }
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
                    if (ProducerV2.send(XMLParser.IntraObjectToXML(user), Severity.AD.ToString()))
                    {
                        Console.WriteLine(XMLParser.IntraObjectToXML(user));
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
            if (Program.Connection == Connection.LOCAL)
            {
                Program = new CRUD();
                Program.Binding(Connection.LDAP);
                lblCurrent.Content = "Current Connection: LDAP";
            }
            else
            {
                Program = new CRUD();
                Program.Binding(Connection.LOCAL);
                lblCurrent.Content = "Current Connection: LOCAL";
            }
            btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
        }

        private void DeleteUserAction(object sender, RoutedEventArgs e)
        {
            if (fieldResults.SelectedIndex != -1)
            {
                Debug.WriteLine(fieldResults.SelectedValue.ToString());

                var user = Program.FindADUser(fieldResults.SelectedValue.ToString()).ADObjectToIntraUserObject();
                user.MetaData = new MetaData { GUID = user.MetaData.GUID, Methode = CRUDMethode.DELETE, Origin = "AD", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") };

                if (ProducerV2.send(XMLParser.IntraObjectToXML(user), Severity.AD.ToString()))
                {
                    Console.WriteLine(XMLParser.IntraObjectToXML(user));
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
                    MessageBox.Show("User succesfully deleted!");
                    btnCreateUser.IsEnabled = btnDeleteUser.IsEnabled = btnUpdateUser.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Select a user first!");
            }
        }

        private void UpdateUserAction(object sender, RoutedEventArgs e)
        {
            if (fieldResults.SelectedIndex != -1)
            {
                var oldUser = Program.FindADUser(fieldResults.SelectedValue.ToString()).ADObjectToIntraUserObject();
                DialogWindow w = new DialogWindow(oldUser);
                w.ShowDialog();

                if (w.DialogResult == true)
                {
                    Debug.WriteLine(fieldResults.SelectedValue.ToString());
                    //try
                    //{
                        var user = w.Answer;
                        if (ProducerV2.send(XMLParser.IntraObjectToXML(user), Severity.AD.ToString()))
                        {
                            Console.WriteLine(XMLParser.IntraObjectToXML(user));

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
            else
            {
                MessageBox.Show("Select a user first!");
            }
        }
    }
}
