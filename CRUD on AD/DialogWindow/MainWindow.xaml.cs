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
        public User Answer { get { return new User { UserData = new UserData { FirstName = txtFirstName.Text, LastName = txtLastName.Text, Email = txtEmail.Text, Role = (rdStudent.IsChecked == true) ? "student" : "docent", Password = txtPassword.Text }, MetaData = Data}; } }

        public DialogWindow()
        {   
            InitializeComponent();

            Data = new MetaData { Methode = CRUDMethode.CREATE, Origin = "AD", TimeStamp = DateTime.Now, UUIDMaster = "NOT SET", GUID = "NOT SET"};

            txtEmail.IsEnabled = false;
            
            btnConfirm.Content = "Create";
        }

        public DialogWindow(User user)
        {
            InitializeComponent();

            Data = new MetaData { Methode = CRUDMethode.UPDATE, Origin = "AD", TimeStamp = DateTime.Now, UUIDMaster = user.MetaData.UUIDMaster, GUID = user.MetaData.GUID};

            rdDocent.IsChecked = rdStudent.IsChecked = false;

            txtFirstName.Text = user.UserData.FirstName;
            txtLastName.Text = user.UserData.LastName;
            txtEmail.Text = user.UserData.Email;
            txtEmail.IsReadOnly = true;
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


    }
}
