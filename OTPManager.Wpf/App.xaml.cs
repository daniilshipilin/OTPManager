namespace OTPManager.Wpf
{
    using System;
    using System.Windows;
    using OTPManager.Wpf.Helpers;
    using OTPManager.Wpf.Views;

    public partial class App : Application
    {
        private static void ShowLoginView()
        {
            var view = new LoginView();
            view.ShowDialog();
        }

        private static void ShowOtpView()
        {
            var view = new OtpView();
            view.ShowDialog();
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                try
                {
                    if (e.Args[0].Equals("-p") && e.Args.Length == 3)
                    {
                        if (OtpKeysFileProcessor.ChangeFileEncryptionPassword(e.Args[1], e.Args[2]))
                        {
                            MessageBox.Show("Encryption password successfully changed");
                        }
                        else
                        {
                            MessageBox.Show("Encryption password change failed");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unknown args detected");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Environment.Exit(0);
            }

            ShowLoginView();

            if (OtpKeysFileProcessor.LoginIsSuccessful)
            {
                ShowOtpView();
            }

            Environment.Exit(0);
        }
    }
}
