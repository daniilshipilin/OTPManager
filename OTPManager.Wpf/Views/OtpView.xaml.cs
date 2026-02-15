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
using Microsoft.Win32;
using OTPManager.Wpf.Helpers;
using OTPManager.Wpf.Models;
using QRCoder;

public partial class OtpView : Window, IDisposable
{
    private readonly DispatcherTimer otpUpdateTimer = new();
    private readonly DispatcherTimer logOffTimer = new();
    private readonly DispatcherTimer infoMessageResetTimer = new();
    private bool infoMessageIsNew;
    private string previousTotpValue = string.Empty;

    public OtpView()
    {
        this.InitializeComponent();
        this.DataContext = this;

        this.SaveRecordCommand = new CommandHandler(this.SaveRecord, canExecute: () => this.Otps.Count > 0);
        this.InsertRecordCommand = new CommandHandler(this.InsertRecord, canExecute: () => true);
        this.DeleteRecordCommand = new CommandHandler(this.DeleteRecord, canExecute: () => this.CanDeleteRecord);
        this.ShowQRCodeCommand = new CommandHandler(this.ShowQRCode, canExecute: () => this.CanShowQRCode);
        this.GenerateRandomBase32KeyCommand = new CommandHandler(this.GenerateRandomBase32Key, canExecute: () => this.CanGenerateRandomBase32Key);
        this.ExportOtpKeysCommand = new CommandHandler(this.ExportOtpKeys, canExecute: () => true);
    }

    public ObservableCollection<OtpObject> Otps { get; set; } = [];

    public OtpObject? SelectedOtp { get; set; }

    public ICommand SaveRecordCommand { get; }

    public ICommand InsertRecordCommand { get; }

    public ICommand DeleteRecordCommand { get; }

    public ICommand ShowQRCodeCommand { get; }

    public ICommand GenerateRandomBase32KeyCommand { get; }

    public ICommand ExportOtpKeysCommand { get; }

    public bool CanDeleteRecord => this.SelectedOtp is not null;

    public bool CanShowQRCode => this.SelectedOtp is not null;

    public bool CanGenerateRandomBase32Key => this.SelectedOtp is not null;

    private void SetupTimers()
    {
        this.otpUpdateTimer.Interval = TimeSpan.FromSeconds(1);
        this.otpUpdateTimer.Tick += this.OtpRefresh;

        this.infoMessageResetTimer.Interval = TimeSpan.FromSeconds(5);
        this.infoMessageResetTimer.Tick += this.ResetInfoMessage;
        this.infoMessageResetTimer.Start();

        this.logOffTimer.Interval = TimeSpan.FromMinutes(1);
        this.logOffTimer.Tick += this.LogOffSession;
        this.logOffTimer.Start();
    }

    private void StopTimers()
    {
        this.otpUpdateTimer.Stop();
        this.otpUpdateTimer.Tick -= this.OtpRefresh;

        this.infoMessageResetTimer.Stop();
        this.infoMessageResetTimer.Tick -= this.ResetInfoMessage;

        this.logOffTimer.Stop();
        this.logOffTimer.Tick -= this.LogOffSession;
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

    private void LogOffSession(object? sender, EventArgs e)
        => this.Close();

    private void OtpRefresh(object? sender, EventArgs e)
    {
        if (this.SelectedOtp is not null)
        {
            string currentValue = this.SelectedOtp.TotpValue;

            if (!this.previousTotpValue.Equals(currentValue))
            {
                int totpHalfSize = this.SelectedOtp.TotpSize / 2;
                this.otpValueTextBlock.Text = $"{currentValue[0..totpHalfSize]} {currentValue[totpHalfSize..]}";
                this.previousTotpValue = currentValue;
            }

            this.progressBar.Value = (this.SelectedOtp.TimeWindowStep - this.SelectedOtp.RemainingSeconds) / (double)this.SelectedOtp.TimeWindowStep * 100;
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
        this.SelectedOtp = null;
        this.previousTotpValue = string.Empty;
        this.ClearTextBoxes();
        this.Otps.Clear();

        foreach (var otpKey in OtpKeysProcessor.LoadData()
                                        .OrderByDescending(x => x.IsFavorite)
                                        .ThenBy(x => x.Description))
        {
            this.Otps.Add(otpKey);
        }

        this.dbRevisionLabel.Content = OtpKeysJSON.Revision;
        this.dbRevisionTimestampLabel.Content = TimestampHelper.UnixTimeStampToDateTime(OtpKeysJSON.LastEditTimestamp).ToString("s");

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

    private void ExportOtpKeys()
    {
        this.logOffTimer.Stop();

        string otpKeysReg = AppSettings.ExportOtpKeysRegValue();
        var saveFileDialog = new SaveFileDialog()
        {
            FileName = "OtpKeys",
            InitialDirectory = Environment.CurrentDirectory,
            Filter = "Registry file (*.reg)|*.reg",
        };

        bool? result = saveFileDialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            File.WriteAllText(saveFileDialog.FileName, otpKeysReg);
            MessageBox.Show($"Otp keys successfully exported to '{saveFileDialog.FileName}'", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        this.logOffTimer.Start();
    }

    private void ClearTextBoxes()
    {
        this.selectedOtpDescriptionTextBox.Text = string.Empty;
        this.selectedOtpBase32SecretKeyTextBox.Text = string.Empty;
        this.otpValueTextBlock.Text = string.Empty;
        this.progressBar.Value = 0;
        this.totalRecordsLabel.Content = string.Empty;
        this.dbRevisionLabel.Content = string.Empty;
        this.dbRevisionTimestampLabel.Content = string.Empty;
    }

    private void InsertRecord()
    {
        this.logOffTimer.Stop();
        var dr = MessageBox.Show("Insert new record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            try
            {
                this.Otps.Add(OtpObject.GetRandomOtpObject());
                OtpKeysProcessor.SaveData(this.Otps);
                this.PrintInfoMessage("Record added");
                this.InitData();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        this.logOffTimer.Start();
    }

    private void SaveRecord()
    {
        this.logOffTimer.Stop();
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
                    OtpKeysProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Record updated");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }

        this.logOffTimer.Start();
    }

    private void DeleteRecord()
    {
        this.logOffTimer.Stop();
        var dr = MessageBox.Show("Delete currently selected record?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            if (this.SelectedOtp is not null)
            {
                try
                {
                    this.Otps.Remove(this.SelectedOtp);
                    OtpKeysProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Record deleted");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }

        this.logOffTimer.Start();
    }

    private void ShowQRCode()
    {
        this.logOffTimer.Stop();

        if (this.SelectedOtp is not null)
        {
            try
            {
                var payload = new PayloadGenerator.OneTimePassword
                {
                    Label = this.SelectedOtp.Description,
                    Secret = this.SelectedOtp.Base32SecretKey,
                    Issuer = "issuer",
                };
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
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        this.logOffTimer.Start();
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
        this.logOffTimer.Stop();
        var dr = MessageBox.Show("Generate random base32 secret key?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dr == MessageBoxResult.Yes)
        {
            if (this.SelectedOtp is not null)
            {
                try
                {
                    this.SelectedOtp.Base32SecretKey = OtpObject.GetRandomBase32String();
                    this.SelectedOtp.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
                    OtpKeysProcessor.SaveData(this.Otps);
                    this.PrintInfoMessage("Base32 secret key generated");
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }

        this.logOffTimer.Start();
    }

    private static void ShowExceptionMessage(Exception ex)
    {
        _ = MessageBox.Show(
            ex.Message,
            ex.GetType().ToString(),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.InitData();
        this.SetupTimers();
        this.programInfoTextBlock.Text = ApplicationInfo.AppHeader;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        this.logOffTimer.Stop();

        if (e.Key == Key.Escape)
        {
            this.Close();
            return;
        }

        this.logOffTimer.Start();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.logOffTimer.Stop();
        this.logOffTimer.Start();
    }

    private void OtpValueTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.logOffTimer.Stop();

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

        this.logOffTimer.Start();
    }

    private void OtpsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        this.logOffTimer.Stop();

        if (this.SelectedOtp is not null)
        {
            this.selectedOtpDescriptionTextBox.Text = this.SelectedOtp.Description;
            this.selectedOtpBase32SecretKeyTextBox.Text = this.SelectedOtp.Base32SecretKey;
            this.OtpRefresh(this, null!);
        }

        this.logOffTimer.Start();
    }

    private void OtpsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        this.logOffTimer.Stop();

        if (e.EditingElement is CheckBox cb)
        {
            if (this.SelectedOtp is not null)
            {
                this.SelectedOtp.IsFavorite = cb.IsChecked is not null && cb.IsChecked.Value;

                try
                {
                    OtpKeysProcessor.SaveData(this.Otps);
                    this.InitData();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }

        this.logOffTimer.Start();
    }

    public void Dispose()
    {
        this.StopTimers();
        this.Otps.Clear();
        this.SelectedOtp = null;
    }
}
