namespace OTPManager.Wpf.Helpers;

using Microsoft.Win32;

public static class AppSettings
{
    public const string RegistryBaseKey = @"Software\Illuminati Software Inc.";

    public const string RegistryOTPManagerKey =
#if DEBUG
        RegistryBaseKey + "\\OTPManager [Debug]";
#else
        RegistryBaseKey + "\\OTPManager";
#endif

    private static readonly RegistryKey RegKeyOTPManager = Registry.CurrentUser.CreateSubKey(RegistryOTPManagerKey);

    public static string OtpKeys
    {
        get => (string?)RegKeyOTPManager.GetValue(nameof(OtpKeys)) ?? string.Empty;

        set => RegKeyOTPManager.SetValue(nameof(OtpKeys), value ?? string.Empty);
    }
}
