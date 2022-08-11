namespace OTPManager.Wpf.Views;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OTPManager.Wpf.Helpers;
using OTPManager.Wpf.Models;
using QRCoder;

public partial class OtpView : Window
{
    private readonly DispatcherTimer otpUpdateTimer = new();
    private readonly DispatcherTimer infoMessageResetTimer = new();
    private bool infoMessageIsNew;

    public OtpView()
    {
        this.InitializeComponent();
        this.DataContext = this;

        this.SaveRecordCommand = new CommandHandler(() => this.SaveRecord(), () => this.Otps.Count > 0);
        this.InsertRecordCommand = new CommandHandler(() => this.InsertRecord(), () => true);
        this.DeleteRecordCommand = new CommandHandler(() => this.DeleteRecord(), () => this.CanDeleteRecord);
        this.ShowQRCodeCommand = new CommandHandler(() => this.ShowQRCode(), () => this.CanShowQRCode);
        this.GenerateRandomBase32KeyCommand = new CommandHandler(() => this.GenerateRandomBase32Key(), () => this.CanGenerateRandomBase32Key);
        this.RefreshRecordsCommand = new CommandHandler(() => this.InitData(), () => true);
    }

    public ObservableCollection<OtpObject> Otps { get; set; } = new ObservableCollection<OtpObject>();

    public OtpObject? SelectedOtp { get; set; }

    public ICommand SaveRecordCommand { get; }

    public ICommand InsertRecordCommand { get; }

    public ICommand DeleteRecordCommand { get; }

    public ICommand ShowQRCodeCommand { get; }

    public ICommand GenerateRandomBase32KeyCommand { get; }

    public ICommand RefreshRecordsCommand { get; }

    public bool CanDeleteRecord => this.SelectedOtp is not null;

    public bool CanShowQRCode => this.SelectedOtp is not null;

    public bool CanGenerateRandomBase32Key => this.SelectedOtp is not null;

    private void SetupTimers()
    {
        this.otpUpdateTimer.Interval = TimeSpan.FromMilliseconds(250);
        this.otpUpdateTimer.Tick += this.OtpRefresh;

        this.infoMessageResetTimer.Interval = TimeSpan.FromSeconds(5);
        this.infoMessageResetTimer.Tick += this.ResetInfoMessage;
        this.infoMessageResetTimer.Start();
    }

    private void ResetInfoMessage(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(this.infoMessageTextBlock.Text) && !this.infoMessageIsNew)
        {
            this.infoMessageTextBlock.Text = string.Empty;
        }
        else
        {
            this.infoMessageIsNew = false;
        }
    }

    private void PrintInfoMessage(string message)
    {
        this.infoMessageTextBlock.Text = message;
        this.infoMessageIsNew = true;
    }

    private void InitData()
    {
        this.otpUpdateTimer.Stop();
        this.ClearTextBoxes();
        this.Otps.Clear();

        foreach (var otpKey in OtpKeysFileProcessor.LoadData().OrderBy(x => x.Description))
        {
            this.Otps.Add(otpKey);
        }

        this.dbRevisionLabel.Content = OtpKeysJSON.FileRevision;
        this.dbRevisionTimestampLabel.Content = TimestampHelper.UnixTimeStampToDateTime(OtpKeysJSON.FileLastEditTimestamp).ToString("s");

        // select first item
        if (this.Otps.Count > 0)
        {
            this.SelectedOtp = this.Otps.First();
            this.selectedOtpDescriptionTextBox.Text = this.SelectedOtp.Description;
            this.selectedOtpBase32SecretKeyTextBox.Text = this.SelectedOtp.Base32SecretKey;
            this.totalRecordsLabel.Content = this.Otps.Count;
            this.otpUpdateTimer.Start();
        }
    }

    private void ClearTextBoxes()
    {
        this.selectedOtpDescriptionTextBox.Text = string.Empty;
        this.selectedOtpBase32SecretKeyTextBox.Text = string.Empty;
        this.otpValueTextBlock.Text = string.Empty;
        this.otpRemainingSecondsTextBlock.Text = string.Empty;
        this.progressBar.Value = 0;
        this.totalRecordsLabel.Content = string.Empty;
        this.dbRevisionLabel.Content = string.Empty;
        this.dbRevisionTimestampLabel.Content = string.Empty;
    }

    private void InsertRecord()
    {
        var dr = MessageBox.Show("Insert new record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            try
            {
                this.Otps.Add(OtpObject.GetRandomOtpObject());
                OtpKeysFileProcessor.SaveData(this.Otps);
                this.PrintInfoMessage("Record added");
                this.InitData();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }
    }

    private void SaveRecord()
    {
        var dr = MessageBox.Show("Save existing records?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            if (this.SelectedOtp is not null)
            {
                try
                {
                    this.SelectedOtp.Description = this.selectedOtpDescriptionTextBox.Text;
                    this.SelectedOtp.Base32SecretKey = this.selectedOtpBase32SecretKeyTextBox.Text;
                    this.SelectedOtp.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
                    OtpKeysFileProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Record updated");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }
    }

    private void DeleteRecord()
    {
        var dr = MessageBox.Show("Delete currently selected record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            if (this.SelectedOtp is not null)
            {
                try
                {
                    this.Otps.Remove(this.SelectedOtp);
                    OtpKeysFileProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Record deleted");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }
    }

    private void ShowQRCode()
    {
        if (this.SelectedOtp is not null)
        {
            try
            {
                var payload = GenerateQRPayload("label", this.SelectedOtp.Base32SecretKey, "issuer");
                var bitmapImage = GenerateQRCode(payload);

                var window = new Window
                {
                    Title = this.SelectedOtp.Description,
                    Width = 500,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                };

                var grid = new Grid();
                grid.Children.Add(new Image { Source = bitmapImage });
                window.Content = grid;
                window.Show();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }
    }

    private static PayloadGenerator.OneTimePassword GenerateQRPayload(string label, string secret, string issuer)
    {
        return new PayloadGenerator.OneTimePassword()
        {
            Label = label,
            Secret = secret,
            Issuer = issuer,
        };
    }

    private static BitmapImage GenerateQRCode(PayloadGenerator.OneTimePassword payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload.ToString(), QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        using var qrCodeImage = qrCode.GetGraphic(20);

        using var memory = new MemoryStream();
        qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;

        var bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
    }

    private void GenerateRandomBase32Key()
    {
        var dr = MessageBox.Show("Generate random base32 secret key?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            if (this.SelectedOtp is not null)
            {
                try
                {
                    this.SelectedOtp.Base32SecretKey = OtpObject.GetRandomBase32String();
                    this.SelectedOtp.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
                    OtpKeysFileProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Base32 secret key generated");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }
    }

    private static void ShowExceptionMessage(Exception ex)
    {
        _ = MessageBox.Show(
            ex.Message,
            ex.GetType().ToString(),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void OtpRefresh(object? sender, EventArgs e)
    {
        if (this.SelectedOtp is not null)
        {
            int totpHalfSize = this.SelectedOtp.TotpSize / 2;
            this.otpValueTextBlock.Text = $"{this.SelectedOtp.TotpValue[0..totpHalfSize]} {this.SelectedOtp.TotpValue[totpHalfSize..]}";
            this.otpRemainingSecondsTextBlock.Text = $"{this.SelectedOtp.RemainingSeconds} sec.";
            this.progressBar.Value = (this.SelectedOtp.TimeWindowStep - this.SelectedOtp.RemainingSeconds) / (double)this.SelectedOtp.TimeWindowStep * 100;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.InitData();
        this.SetupTimers();
        this.programInfoTextBlock.Text = ApplicationInfo.AppHeader;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

    private void OtpValueTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (this.SelectedOtp is not null)
        {
            try
            {
                Clipboard.SetDataObject(this.SelectedOtp.TotpValue);
                this.PrintInfoMessage("Otp value copied to the clipboard");
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }
    }

    private void OtpsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (this.SelectedOtp is not null)
        {
            this.selectedOtpDescriptionTextBox.Text = this.SelectedOtp.Description;
            this.selectedOtpBase32SecretKeyTextBox.Text = this.SelectedOtp.Base32SecretKey;
        }
    }
}
