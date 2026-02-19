namespace OTPManager.Wpf;

using System;
using System.Windows;
using OTPManager.Wpf.Helpers;
using OTPManager.Wpf.Models;
using OTPManager.Wpf.Views;

public partial class App : Application
{
    private async void ApplicationStartup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            try
            {
                if (e.Args[0].Equals("-p") && e.Args.Length == 3)
                {
                    if (OtpKeysProcessor.ChangeEncryptionPassword(e.Args[1], e.Args[2]))
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

        bool timeIsSynced = false;
        using var timeSyncTask = NetworkTimeProvider.GetNetworkTimeAsync();

        while (true)
        {
            using var loginView = new LoginView();
            loginView.ShowDialog();

            if (loginView.LoginIsSuccessful)
            {
                await timeSyncTask;

                if (!timeIsSynced && timeSyncTask.IsCompletedSuccessfully && timeSyncTask.Result.HasValue)
                {
                    OtpObject.SetTimeCorrection(timeSyncTask.Result.Value);
                    timeIsSynced = true;
                }

                using var otpView = new OtpView(timeIsSynced);
                otpView.ShowDialog();
            }
            else
            {
                break;
            }
        }

        Environment.Exit(0);
    }
}
