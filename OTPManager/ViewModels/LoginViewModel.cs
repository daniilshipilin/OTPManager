using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using OTPManager.Helpers;
using PropertyChanged;

namespace OTPManager.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class LoginViewModel : INotifyPropertyChanged
    {
        bool _infoMessageIsNew;
        readonly DispatcherTimer _timer;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public string WindowTitle => "OTPManager Login";

        public static bool LoginIsSuccessful { get; private set; }
        public string Password { private get; set; } = string.Empty;
        public string InfoMessage { get; set; }
        public ICommand LoginCommand { get; }
        bool CanLogin => (Password.Length != 0);
        public ICommand EscapeCommand { get; }
        //public Visibility WindowVisibility { get; set; } = Visibility.Visible;
        public Action RequestClose { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(param => Login(), param => CanLogin);
            EscapeCommand = new RelayCommand(param => RequestClose());

            _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 5000) };
            _timer.Tick += ResetInfoMessage;
            _timer.Start();
        }

        private void ResetInfoMessage(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(InfoMessage) && !_infoMessageIsNew)
            {
                InfoMessage = string.Empty;
            }
            else
            {
                _infoMessageIsNew = false;
            }
        }

        private void PrintMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }

            InfoMessage = message;
            _infoMessageIsNew = true;
        }

        private void Login()
        {
            try
            {
                SQLiteDBAccess.Instance = new SQLiteDBAccess(Password, true);
                LoginIsSuccessful = true;
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message);
            }

            if (LoginIsSuccessful)
            {
                RequestClose();
            }
        }
    }
}
