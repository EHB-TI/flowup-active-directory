using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Lib;

namespace InputWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public MetaData Data { get; set; }
        public IntraUser User { get; set; }
        public int Version { get; set; }
        public bool Create { get; set; }    
        public IntraUser Answer { get { return new IntraUser { 
                                        UserData = new UserData 
                                        { 
                                            FirstName = txtFirstName.Text.Equals(string.Empty)
                                                ? "Not Set" : txtFirstName.Text, 
                                            LastName = txtLastName.Text.Equals(string.Empty)
                                                ? "Not Set" : txtLastName.Text, 
                                            Email = txtEmail.Text.Equals(string.Empty)
                                                ? "Not Set" : txtEmail.Text, 
                                            BirthDay = DateTime.Parse(txtBirthday.SelectedDate.ToString()).ToString("yyyy-MM-dd"), 
                                            Study = txtStudy.Text.Equals(string.Empty) 
                                                ? "Not Set" : txtStudy.Text,
                                            Role = rdStudent.IsChecked is true
                                                ? "student" : rdDocent.IsChecked is true ? "tutor": "Not Set",
                                            Password = txtPassword.Text.Equals(string.Empty)
                                                ? Create
                                                    ? "Not Set"
                                                    : User.UserData.Password
                                                : txtPassword.Text
                                        }, 
                                        MetaData = Data
                                    }; 
            } 
        }

        public DialogWindow()
        {   
            InitializeComponent();
            Create = true;

            Data = new MetaData
            {
                Methode = CRUDMethode.CREATE,
                Origin = "AD",
                TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"),
                GUID = "NOT SET",
                Version = 1,
                UUIDMaster = string.Empty
            };

            txtEmail.IsEnabled = false;
            
            btnConfirm.Content = "Create";
        }

        public DialogWindow(IntraUser user)
        {
            InitializeComponent();

            User = user;
            Create = false;
            Data = new MetaData 
            { 
                Methode = CRUDMethode.UPDATE, 
                Origin = "AD", 
                TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"), 
                GUID = User.MetaData.GUID, 
                Version = User.MetaData.Version + 1
            };

            rdDocent.IsChecked = rdStudent.IsChecked = false;

            txtFirstName.Text = User.UserData.FirstName;
            txtLastName.Text = User.UserData.LastName;

            txtEmail.Text = (User.UserData.Email.Length != 0)? User.UserData.Email: 
                (User.UserData.LastName.Length != 0)
                ? $"{User.UserData.FirstName.ToLowerInvariant()}.{User.UserData.LastName.ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com"
                : $"{User.UserData.FirstName.ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com";
            txtEmail.IsReadOnly = true;

            txtBirthday.SelectedDate = (User.UserData.BirthDay != "Not Set") 
                ? DateTime.Parse(User.UserData.BirthDay)
                : DateTime.Parse("1/1/2000");

            txtStudy.Text = User.UserData.Study;
            if (User.UserData.Role == "student") { rdStudent.IsChecked = true; } else { rdDocent.IsChecked = true; }
            txtPassword.Text = User.UserData.Password;

            btnConfirm.Content = "Update";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Create)
            {
                if (!txtFirstName.Text.Equals(string.Empty) && !txtPassword.Text.Equals(string.Empty))
                {
                    if (txtPassword.Text.Count(char.IsDigit) >= 1 && txtPassword.Text.Length >= 7)
                    {
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Password needs to be 7 character long; with atleast 1 number!");
                    }
                }
                else
                {
                    MessageBox.Show("Firstname and Password are REQUIRED!");
                }
            }
            else
            {
                if (txtPassword.Text.Equals(string.Empty) || (txtPassword.Text.Count(char.IsDigit) >= 1 && txtPassword.Text.Length >= 7))
                {
                    if (!txtFirstName.Text.Equals(string.Empty))
                    {
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Firstname is REQUIRED!");
                    }
                }
                else if (txtPassword.Text.Count(char.IsDigit) < 1 || txtPassword.Text.Length < 7)
                {
                    MessageBox.Show("Password needs to be 7 character long; with atleast 1 number!");
                }
            }
            
        }

        private void CancelAction(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ChangedInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtLastName.Text.Length != 0 )
            {
                if (txtFirstName.Text.Contains(" ") || txtLastName.Text.Contains(" "))
                {
                    txtEmail.Text = $"{txtFirstName.Text.Replace(" ", ".").ToLowerInvariant()}.{txtLastName.Text.Replace(" ", ".").ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com";
                }
                else
                {
                    txtEmail.Text = $"{txtFirstName.Text.ToLowerInvariant()}.{txtLastName.Text.ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com";
                }
            } 
            else
            {
                if (txtFirstName.Text.Contains(" "))
                {
                    txtEmail.Text = $"{txtFirstName.Text.Replace(" ", ".").ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com";
                }
                else
                {
                    txtEmail.Text = $"{txtFirstName.Text.ToLowerInvariant()}@flowupdesiderius.onmicrosoft.com";
                }
            }
        }
    }
}
