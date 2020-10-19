using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OTPManager.DBModels;
using OTPManager.Helpers;
using OTPManager.Models;

namespace OTPManager.ViewModels
{
    public class OtpViewModel : INotifyPropertyChanged
    {
        const int DEFAULT_BASE32_SECRET_KEY_SIZE = 32;

        readonly DispatcherTimer _otpUpdateTimer;
        readonly DispatcherTimer _infoMessageResetTimer;
        bool _infoMessageIsNew;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public string WindowTitle => "OTPManager: Time-based One-time Password generator";
        public string ProgramInfo => GitVersionInformation.InformationalVersion;

        public ICommand EscapeCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand UpdateDBRecordCommand { get; }
        public ICommand InsertDBRecordCommand { get; }
        public ICommand DeleteDBRecordCommand { get; }
        public ICommand RefreshDBRecordsCommand { get; }
        public Action RequestClose { get; set; }

        public ObservableCollection<OtpObject> Otps { get; set; } = new ObservableCollection<OtpObject>();
        public OtpObject SelectedOtp { get; set; }
        bool CanCopyOtpValue => TotpValue.Length != 0;
        bool CanUpdateDBRecord => Otps.Count > 0 && SelectedOtp != null;
        bool CanInsertDBRecord => true;
        bool CanDeleteDBRecord => Otps.Count > 0 && SelectedOtp != null;
        public string InfoMessage { get; set; }
        public string TotpValue { get; set; } = string.Empty;
        public string RemainingSeconds { get; set; }
        public double ProgressBarValue { get; set; }

        public OtpViewModel()
        {
            EscapeCommand = new RelayCommand(param => RequestClose());
            CopyCommand = new RelayCommand(param => CopyOtpValue(), param => CanCopyOtpValue);
            UpdateDBRecordCommand = new RelayCommand(param => UpdateDBRecord(), param => CanUpdateDBRecord);
            InsertDBRecordCommand = new RelayCommand(param => InsertDBRecord(), param => CanInsertDBRecord);
            DeleteDBRecordCommand = new RelayCommand(param => DeleteDBRecord(), param => CanDeleteDBRecord);
            RefreshDBRecordsCommand = new RelayCommand(param => InitData());

            InitData();
            OtpRefresh(null, null);

            _otpUpdateTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 1000) };
            _otpUpdateTimer.Tick += OtpRefresh;
            _otpUpdateTimer.Start();

            _infoMessageResetTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 5000) };
            _infoMessageResetTimer.Tick += ResetInfoMessage;
            _infoMessageResetTimer.Start();
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

            InfoMessage = message.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            _infoMessageIsNew = true;
        }

        private void InitData()
        {
            Otps.Clear();

            foreach (var otpKey in SQLiteDBAccess.Instance.Select_OtpKeys())
            {
                Otps.Add(new OtpObject(otpKey.ID, otpKey.Description, otpKey.Base32SecretKey));
            }

            // select first item
            if (Otps.Count > 0)
            {
                SelectedOtp = Otps[0];
            }

            TotpValue = string.Empty;
            RemainingSeconds = string.Empty;
            ProgressBarValue = 0;
        }

        private void InsertDBRecord()
        {
            try
            {
                // insert
                SQLiteDBAccess.Instance.Insert_OtpKeys(new OtpKeys()
                {
                    Description = "Example key",
                    Base32SecretKey = Utils.GenerateRandomBase32String(DEFAULT_BASE32_SECRET_KEY_SIZE)
                });

                PrintMessage("DB record inserted");

                // refresh
                InitData();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message);
            }
        }

        private void UpdateDBRecord()
        {
            try
            {
                // validate first
                if (!OtpObject.Base32SecretKeyIsValid(SelectedOtp.Base32SecretKey))
                {
                    PrintMessage($"'{nameof(OtpObject.Base32SecretKey)}' not valid");
                    return;
                }

                // update
                SQLiteDBAccess.Instance.Update_OtpKeys(new OtpKeys()
                {
                    ID = SelectedOtp.ID,
                    Description = SelectedOtp.Description,
                    Base32SecretKey = SelectedOtp.Base32SecretKey
                });

                PrintMessage("DB record updated");

                // refresh
                InitData();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message);
            }
        }

        private void DeleteDBRecord()
        {
            try
            {
                // delete
                SQLiteDBAccess.Instance.Delete_OtpKeys(new OtpKeys()
                {
                    ID = SelectedOtp.ID,
                    Description = SelectedOtp.Description,
                    Base32SecretKey = SelectedOtp.Base32SecretKey
                });

                PrintMessage("DB record deleted");

                // refresh
                InitData();
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message);
            }
        }

        private void OtpRefresh(object sender, EventArgs e)
        {
            if (Otps.Count > 0 && SelectedOtp != null)
            {
                TotpValue = SelectedOtp.TotpValue;
                RemainingSeconds = $"{SelectedOtp.RemainingSeconds} sec.";
                ProgressBarValue = (SelectedOtp.TimeWindowStep - SelectedOtp.RemainingSeconds)
                    / (double)SelectedOtp.TimeWindowStep * 100;
            }
        }

        private void CopyOtpValue()
        {
            try
            {
                Clipboard.SetDataObject(TotpValue);
                PrintMessage("Otp value copied to the clipboard");
            }
            catch (Exception ex)
            {
                PrintMessage(ex.Message);
            }
        }
    }
}
