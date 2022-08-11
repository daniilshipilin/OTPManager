namespace OTPManager.Wpf.Views;

using System;
using System.Windows;
using System.Windows.Input;
using OTPManager.Wpf.Helpers;

public partial class LoginView : Window
{
    public LoginView()
    {
        this.InitializeComponent();
    }

    private void Login()
    {
        try
        {
            if (OtpKeysFileProcessor.TryReadFile())
            {
                this.Close();
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                ex.GetType().ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(
            "The given encryption password does not match the one currently in use. Please provide the current encryption password.",
            "Login failed",
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.loginButton.IsEnabled = false;
        this.passwordBox.Focus();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        OtpKeysFileProcessor.SetPassword(passwordBox.Password);
        this.Login();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        this.loginButton.IsEnabled = !string.IsNullOrEmpty(passwordBox.Password);
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrEmpty(passwordBox.Password))
        {
            OtpKeysFileProcessor.SetPassword(passwordBox.Password);
            this.Login();
        }
    }
}
