using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OTPManager.DBModels;
using OTPManager.Helpers;
using OTPManager.Models;

namespace OTPManager.ViewModels
{
    public class OtpViewModel : ViewModelBase, INotifyPropertyChanged
    {
        const int DEFAULT_BASE32_SECRET_KEY_SIZE = 32;

        readonly DispatcherTimer _otpUpdateTimer;
        readonly DispatcherTimer _infoMessageResetTimer;
        bool _infoMessageIsNew;

        public string WindowTitle { get; } = "OTPManager: Time-based One-time Password generator";
        public string ProgramInfo { get; } = GitVersionInformation.InformationalVersion;

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
        bool CanUpdateDBRecord => Otps.Count > 0 && SelectedOtp is object;
        bool CanInsertDBRecord => true;
        bool CanDeleteDBRecord => Otps.Count > 0 && SelectedOtp is object;
        public string InfoMessage { get; set; }
        public string TotpValue { get; set; } = string.Empty;
        public string RemainingSeconds { get; set; }
        public double ProgressBarValue { get; set; }

        public OtpViewModel()
        {
            EscapeCommand = new RelayCommand(param => RequestClose());
            CopyCommand = new RelayCommand(param => CopyOtpValue(), param => CanCopyOtpValue);
            UpdateDBRecordCommand = new RelayCommand(param => PromptUpdate("Update existing record?"), param => CanUpdateDBRecord);
            InsertDBRecordCommand = new RelayCommand(param => PromptInsert("Insert new record?"), param => CanInsertDBRecord);
            DeleteDBRecordCommand = new RelayCommand(param => PromptDelete("Delete existing record?"), param => CanDeleteDBRecord);
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
                SelectedOtp = Otps.First();
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

        protected void PromptUpdate(string message)
        {
            MessageBox_Show(ProcessUpdatePrompt, message, "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        protected void PromptInsert(string message)
        {
            MessageBox_Show(ProcessInsertPrompt, message, "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        protected void PromptDelete(string message)
        {
            MessageBox_Show(ProcessDeletePrompt, message, "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public void ProcessUpdatePrompt(MessageBoxResult result)
        {
            if (result == MessageBoxResult.Yes)
            {
                UpdateDBRecord();
            }
        }

        public void ProcessInsertPrompt(MessageBoxResult result)
        {
            if (result == MessageBoxResult.Yes)
            {
                InsertDBRecord();
            }
        }

        public void ProcessDeletePrompt(MessageBoxResult result)
        {
            if (result == MessageBoxResult.Yes)
            {
                DeleteDBRecord();
            }
        }
    }
}
