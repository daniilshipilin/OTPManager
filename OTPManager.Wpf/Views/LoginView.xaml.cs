namespace OTPManager.Wpf.Views;

using System;
using System.Windows;
using System.Windows.Input;
using OTPManager.Wpf.Helpers;

public partial class LoginView : Window, IDisposable
{
    private bool disposedValue;

    public bool LoginIsSuccessful { get; private set; }

    public LoginView()
    {
        this.InitializeComponent();
        OtpKeysFileProcessor.ResetPassword();
    }

    private void Login()
    {
        try
        {
            if (OtpKeysFileProcessor.TryReadFile())
            {
                this.LoginIsSuccessful = true;
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

        this.passwordBox.Clear();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.loginButton.IsEnabled = false;
        this.passwordBox.Focus();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        OtpKeysFileProcessor.SetPassword(this.passwordBox.Password);
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
        this.loginButton.IsEnabled = !string.IsNullOrEmpty(this.passwordBox.Password);
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrEmpty(this.passwordBox.Password))
        {
            OtpKeysFileProcessor.SetPassword(this.passwordBox.Password);
            this.Login();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            this.disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LoginView()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
