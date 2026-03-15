namespace OTPManager.Wpf;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using OTPManager.Wpf.Helpers;
using OTPManager.Wpf.Models;
using OTPManager.Wpf.Views;

public partial class App : Application
{
    private async void ApplicationStartup(object sender, StartupEventArgs e)
    {
        if (CheckAnotherInstanceIsRunning(ApplicationInfo.AppTitle))
        {
            MessageBox.Show($"Another instance of '{ApplicationInfo.AppTitle}' is running", ApplicationInfo.AppHeader, MessageBoxButton.OK, MessageBoxImage.Warning);
            ProgramExit(0);
        }

        while (true)
        {
            using var timeSyncTask = SyncTimeAsync();
            using var loginView = new LoginView();
            loginView.ShowDialog();

            if (loginView.LoginIsSuccessful)
            {
                await timeSyncTask;
                using var otpView = new OtpView(timeSyncTask.IsCompletedSuccessfully);
                otpView.ShowDialog();

                if (!otpView.IsLogOffSequence)
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        ProgramExit(0);
    }

    private static bool CheckAnotherInstanceIsRunning(string? programName)
        => Process.GetProcessesByName(programName).Length > 1;

    private static void ProgramExit(int exitCode)
        => Environment.Exit(exitCode);

    private static async Task SyncTimeAsync()
    {
        var time = await NetworkTimeProvider.GetNetworkTimeAsync();

        if (time.HasValue)
        {
            OtpObject.SetTimeCorrection(time.Value);
        }
    }
}
