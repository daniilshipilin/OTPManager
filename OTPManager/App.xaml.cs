using System;
using System.Windows;
using OTPManager.ViewModels;
using OTPManager.Views;

namespace OTPManager
{
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            {
                var view = new LoginView();
                var vm = new LoginViewModel();
                vm.RequestClose += delegate { view.Close(); };
                view.DataContext = vm;
                view.ShowDialog();
            }

            if (LoginViewModel.LoginIsSuccessful)
            {
                var view = new OtpView();
                var vm = new OtpViewModel();
                vm.RequestClose += delegate { view.Close(); };
                view.DataContext = vm;
                view.ShowDialog();
            }

            Environment.Exit(0);
        }
    }
}
