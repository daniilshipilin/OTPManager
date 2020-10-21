using System;
using System.ComponentModel;
using System.Windows;
using OTPManager.Helpers;

namespace OTPManager.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<MvvmMessageBoxEventArgs> MessageBoxRequest;

        protected void MessageBox_Show(Action<MessageBoxResult> resultAction,
                                       string messageBoxText,
                                       string caption = "",
                                       MessageBoxButton button = MessageBoxButton.OK,
                                       MessageBoxImage icon = MessageBoxImage.None,
                                       MessageBoxResult defaultResult = MessageBoxResult.None,
                                       MessageBoxOptions options = MessageBoxOptions.None)
        {
            MessageBoxRequest?.Invoke(this, new MvvmMessageBoxEventArgs(resultAction, messageBoxText, caption, button, icon, defaultResult, options));
        }
    }
}
