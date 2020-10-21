using System;
using System.Windows;
using OTPManager.Helpers;
using OTPManager.ViewModels;

namespace OTPManager.Views
{
    public partial class OtpView : Window
    {
        public OtpView()
        {
            InitializeComponent();
        }

        void OtpView_MessageBoxRequest(object sender, MvvmMessageBoxEventArgs e)
        {
            e.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is object)
            {
                ((OtpViewModel)DataContext).MessageBoxRequest += new EventHandler<MvvmMessageBoxEventArgs>(OtpView_MessageBoxRequest);
            }
        }
    }
}
