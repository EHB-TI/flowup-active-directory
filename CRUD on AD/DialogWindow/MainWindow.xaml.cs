using System.Windows;
using Lib;

namespace InputWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public User Answer { get { return new User { UserData = new UserData { FirstName = txtFirstName.Text, LastName = txtLastName.Text, Email = txtEmail.Text, Role = (rdStudent.IsChecked == true) ? "student" : "docent", Password = txtPassword.Text } }; } }

        public DialogWindow()
        {
            InitializeComponent();

            btnConfirm.Content = "Create";
        }

        public DialogWindow(User user)
        {
            InitializeComponent();


            rdDocent.IsChecked = rdStudent.IsChecked = false;

            txtFirstName.Text = user.UserData.FirstName;
            txtLastName.Text = user.UserData.LastName;
            txtEmail.Text = user.UserData.Email;
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
