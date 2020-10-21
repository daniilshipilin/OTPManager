using System;
using System.Windows;

namespace OTPManager.Helpers
{
    public class MvvmMessageBoxEventArgs : EventArgs
    {
        public MvvmMessageBoxEventArgs(Action<MessageBoxResult> resultAction,
                                       string messageBoxText,
                                       string caption = "",
                                       MessageBoxButton button = MessageBoxButton.OK,
                                       MessageBoxImage icon = MessageBoxImage.None,
                                       MessageBoxResult defaultResult = MessageBoxResult.None,
                                       MessageBoxOptions options = MessageBoxOptions.None)
        {
            _resultAction = resultAction;
            _messageBoxText = messageBoxText;
            _caption = caption;
            _button = button;
            _icon = icon;
            _defaultResult = defaultResult;
            _options = options;
        }

        readonly Action<MessageBoxResult> _resultAction;

        readonly string _messageBoxText;
        readonly string _caption;
        readonly MessageBoxButton _button;
        readonly MessageBoxImage _icon;
        readonly MessageBoxResult _defaultResult;
        readonly MessageBoxOptions _options;

        public void Show(Window owner)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(owner, _messageBoxText, _caption, _button, _icon, _defaultResult, _options);
            _resultAction?.Invoke(messageBoxResult);
        }

        public void Show()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(_messageBoxText, _caption, _button, _icon, _defaultResult, _options);
            _resultAction?.Invoke(messageBoxResult);
        }
    }
}
