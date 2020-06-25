using System.Windows;
using System.Windows.Controls;
using OTPManager.ViewModels;

namespace OTPManager.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            passwordBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginViewModel)DataContext).Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
