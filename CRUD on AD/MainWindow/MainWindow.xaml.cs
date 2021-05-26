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
                    if (ProducerV2.send(XMLParser.ObjectToXML(user), Severity.AD.ToString()))
                    {
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

                var user = Program.FindADUser(fieldResults.SelectedValue.ToString()).ADObjectToUserObject();
                user.MetaData = new MetaData { GUID = user.MetaData.GUID, Methode = CRUDMethode.DELETE, Origin = "AD", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") };

                if (ProducerV2.send(XMLParser.ObjectToXML(user), Severity.AD.ToString()))
                {
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
                var oldUser = Program.FindADUser(fieldResults.SelectedValue.ToString()).ADObjectToUserObject();
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
