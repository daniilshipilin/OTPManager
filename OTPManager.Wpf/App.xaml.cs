namespace OTPManager.Wpf;

using System;
using System.Threading.Tasks;
using System.Windows;
using ApplicationUpdater;
using OTPManager.Wpf.Helpers;
using OTPManager.Wpf.Views;

public partial class App : Application
{
    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        AppSettings.CheckSettings();

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

        // check for updates in the background
        var updateTask = Task.Run(CheckUpdates);

        while (true)
        {
            var loginView = new LoginView();
            loginView.ShowDialog();

            if (loginView.LoginIsSuccessful)
            {
                using var otpView = new OtpView();
                otpView.ShowDialog();
            }
            else
            {
                break;
            }
        }

        if (!updateTask.IsCompleted)
        {
            updateTask.Wait();
        }

        Environment.Exit(0);
    }

    private static async Task CheckUpdates()
    {
        if ((DateTime.Now - AppSettings.UpdatesLastCheckedTimestamp).Days >= 1)
        {
            try
            {
                var updater = new Updater(
                ApplicationInfo.BaseDirectory,
                ApplicationInfo.AppVersion,
                ApplicationInfo.AppGUID,
                ApplicationInfo.ExePath);

                AppSettings.UpdateUpdatesLastCheckedTimestamp();

                if (await updater.CheckUpdateIsAvailable())
                {
                    var dr = MessageBox.Show(
                        updater.GetUpdatePrompt(),
                        "Program update",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (dr == MessageBoxResult.Yes)
                    {
                        await updater.Update();
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }
    }

    private static void ShowExceptionMessage(Exception ex)
    {
        MessageBox.Show(
            ex.Message,
            ex.GetType().ToString(),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
