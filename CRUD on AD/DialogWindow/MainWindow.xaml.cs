using System;
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
        public int Version { get; set; }    
        public User Answer { get { return new User { UserData = new UserData { FirstName = txtFirstName.Text, LastName = txtLastName.Text, Email = txtEmail.Text, BirthDay = txtBirthday.SelectedDate.ToString(), Study = txtStudy.Text ,Role = (rdStudent.IsChecked == true) ? "student" : "tutor", Password = txtPassword.Text }, MetaData = Data}; } }

        public DialogWindow()
        {   
            InitializeComponent();

            Data = new MetaData { Methode = CRUDMethode.CREATE, Origin = "AD", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"), GUID = "NOT SET", Version = 1};

            txtEmail.IsEnabled = false;
            
            btnConfirm.Content = "Create";
        }

        public DialogWindow(User user)
        {
            InitializeComponent();

            Data = new MetaData { Methode = CRUDMethode.UPDATE, Origin = "AD", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"), GUID = user.MetaData.GUID, Version = user.MetaData.Version + 1};

            rdDocent.IsChecked = rdStudent.IsChecked = false;

            txtFirstName.Text = user.UserData.FirstName;
            txtLastName.Text = user.UserData.LastName;
            txtEmail.Text = user.UserData.Email;
            txtEmail.IsReadOnly = true;
            txtBirthday.SelectedDate = DateTime.Parse(user.UserData.BirthDay);
            txtStudy.Text = user.UserData.Study;
            if (user.UserData.Role == "student") { rdStudent.IsChecked = true; } else { rdDocent.IsChecked = true; }
            txtPassword.Text = user.UserData.Password;

            btnConfirm.Content = "Update";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelAction(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ChangedInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Console.WriteLine("Changed");
            if (txtLastName.Text.Length != 0 )
            {
                txtEmail.Text = $"{txtFirstName.Text.ToLowerInvariant()}.{txtLastName.Text.ToLowerInvariant()}@student.dhs.be";
            } 
            else
            {
                txtEmail.Text = $"{txtFirstName.Text.ToLowerInvariant()}@student.dhs.be";
            }
        }
    }
}
