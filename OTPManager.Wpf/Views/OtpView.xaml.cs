namespace OTPManager.Wpf.Views
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using OTPManager.Wpf.Helpers;
    using OTPManager.Wpf.Models;

    public partial class OtpView : Window
    {
        private readonly DispatcherTimer otpUpdateTimer = new();
        private readonly DispatcherTimer infoMessageResetTimer = new();
        private bool infoMessageIsNew;

        public OtpView()
        {
            InitializeComponent();
            DataContext = this;

            SaveRecordCommand = new CommandHandler(() => SaveRecord(), () => true);
            InsertRecordCommand = new CommandHandler(() => InsertRecord(), () => true);
            DeleteRecordCommand = new CommandHandler(() => DeleteRecord(), () => CanDeleteRecord);
            GenerateRandomBase32KeyCommand = new CommandHandler(() => GenerateRandomBase32Key(), () => CanGenerateRandomBase32Key);
            RefreshRecordsCommand = new CommandHandler(() => InitData(), () => true);
        }

        public ObservableCollection<OtpObject> Otps { get; set; } = new ObservableCollection<OtpObject>();

        public OtpObject? SelectedOtp { get; set; }

        public ICommand SaveRecordCommand { get; }

        public ICommand InsertRecordCommand { get; }

        public ICommand DeleteRecordCommand { get; }

        public ICommand GenerateRandomBase32KeyCommand { get; }

        public ICommand RefreshRecordsCommand { get; }

        public bool CanDeleteRecord => SelectedOtp is not null;

        public bool CanGenerateRandomBase32Key => SelectedOtp is not null;

        private void SetupTimers()
        {
            otpUpdateTimer.Interval = TimeSpan.FromMilliseconds(250);
            otpUpdateTimer.Tick += OtpRefresh;

            infoMessageResetTimer.Interval = TimeSpan.FromSeconds(5);
            infoMessageResetTimer.Tick += ResetInfoMessage;
            infoMessageResetTimer.Start();
        }

        private void ResetInfoMessage(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(infoMessageTextBlock.Text) && !infoMessageIsNew)
            {
                infoMessageTextBlock.Text = string.Empty;
            }
            else
            {
                infoMessageIsNew = false;
            }
        }

        private void PrintInfoMessage(string message)
        {
            infoMessageTextBlock.Text = message;
            infoMessageIsNew = true;
        }

        private void InitData()
        {
            otpUpdateTimer.Stop();
            ClearTextBoxes();
            Otps.Clear();

            foreach (var otpKey in OtpKeysFileProcessor.LoadData().OrderBy(x => x.Description))
            {
                Otps.Add(otpKey);
            }

            // select first item
            if (Otps.Count > 0)
            {
                SelectedOtp = Otps.First();
                selectedOtpDescriptionTextBox.Text = SelectedOtp.Description;
                selectedOtpBase32SecretKeyTextBox.Text = SelectedOtp.Base32SecretKey;
                totalRecordsLabel.Content = Otps.Count;
                otpUpdateTimer.Start();
            }
        }

        private void ClearTextBoxes()
        {
            selectedOtpDescriptionTextBox.Text = string.Empty;
            selectedOtpBase32SecretKeyTextBox.Text = string.Empty;
            otpValueTextBlock.Text = string.Empty;
            otpRemainingSecondsTextBlock.Text = string.Empty;
            progressBar.Value = 0;
            totalRecordsLabel.Content = string.Empty;
        }

        private void InsertRecord()
        {
            var dr = MessageBox.Show("Insert new record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dr == MessageBoxResult.Yes)
            {
                try
                {
                    Otps.Add(OtpObject.GetRandomOtpObject());
                    OtpKeysFileProcessor.SaveData(Otps);
                    PrintInfoMessage("Record added");
                    InitData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveRecord()
        {
            var dr = MessageBox.Show("Save existing records?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dr == MessageBoxResult.Yes)
            {
                if (SelectedOtp is not null)
                {
                    try
                    {
                        SelectedOtp.Description = selectedOtpDescriptionTextBox.Text;
                        SelectedOtp.Base32SecretKey = selectedOtpBase32SecretKeyTextBox.Text;
                        SelectedOtp.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
                        OtpKeysFileProcessor.SaveData(Otps);
                        PrintInfoMessage("Record updated");
                        InitData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteRecord()
        {
            var dr = MessageBox.Show("Delete currently selected record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dr == MessageBoxResult.Yes)
            {
                if (SelectedOtp is not null)
                {
                    try
                    {
                        Otps.Remove(SelectedOtp);
                        OtpKeysFileProcessor.SaveData(Otps);
                        PrintInfoMessage("Record deleted");
                        InitData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void GenerateRandomBase32Key()
        {
            var dr = MessageBox.Show("Generate random base32 secret key?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dr == MessageBoxResult.Yes)
            {
                if (SelectedOtp is not null)
                {
                    try
                    {
                        SelectedOtp.Base32SecretKey = OtpObject.GetRandomBase32String();
                        SelectedOtp.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
                        OtpKeysFileProcessor.SaveData(Otps);
                        PrintInfoMessage("Base32 secret key generated");
                        InitData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OtpRefresh(object? sender, EventArgs e)
        {
            if (SelectedOtp is not null)
            {
                int totpHalfSize = SelectedOtp.TotpSize / 2;
                otpValueTextBlock.Text = $"{SelectedOtp.TotpValue[0..totpHalfSize]} {SelectedOtp.TotpValue[totpHalfSize..]}";
                otpRemainingSecondsTextBlock.Text = $"{SelectedOtp.RemainingSeconds} sec.";
                progressBar.Value = (SelectedOtp.TimeWindowStep - SelectedOtp.RemainingSeconds) / (double)SelectedOtp.TimeWindowStep * 100;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitData();
            SetupTimers();
            programInfoTextBlock.Text = $"v{GitVersionInformation.SemVer}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OtpValueTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedOtp is not null)
            {
                try
                {
                    Clipboard.SetDataObject(SelectedOtp.TotpValue);
                    PrintInfoMessage("Otp value copied to the clipboard");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OtpsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (SelectedOtp is not null)
            {
                selectedOtpDescriptionTextBox.Text = SelectedOtp.Description;
                selectedOtpBase32SecretKeyTextBox.Text = SelectedOtp.Base32SecretKey;
            }
        }
    }
}
