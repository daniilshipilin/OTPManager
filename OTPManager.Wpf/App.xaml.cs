namespace OTPManager.Wpf
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using ApplicationUpdater;
    using OTPManager.Wpf.Helpers;
    using OTPManager.Wpf.Views;

    public partial class App : Application
    {
        private IUpdater? updater;

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
            AppSettings.CheckSettings();

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

            // init program updater
            InitUpdater();

            // check for updates in the background
            Task.Run(async () => await CheckUpdates());

            ShowLoginView();

            if (OtpKeysFileProcessor.LoginIsSuccessful)
            {
                ShowOtpView();
            }

            Environment.Exit(0);
        }

        private void InitUpdater()
        {
            try
            {
                updater = new Updater(
                    ApplicationInfo.BaseDirectory,
                    Version.Parse(GitVersionInformation.SemVer),
                    ApplicationInfo.AppGUID,
                    ApplicationInfo.ExePath);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private async Task CheckUpdates()
        {
            if (updater is null)
            {
                return;
            }

            if ((DateTime.Now - AppSettings.UpdatesLastCheckedTimestamp).Days >= 1)
            {
                try
                {
                    AppSettings.UpdateUpdatesLastCheckedTimestamp();

                    if (await updater.CheckUpdateIsAvailable())
                    {
                        var dr = MessageBox.Show(
                            updater.GetUpdatePrompt(),
                            "Program update",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
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
}
