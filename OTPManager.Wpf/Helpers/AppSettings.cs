namespace OTPManager.Wpf.Helpers;

using System.Text;
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

    public static string ExportOtpKeysRegValue()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Windows Registry Editor Version 5.00");
        sb.AppendLine(string.Empty);
        sb.AppendLine($"[{RegKeyOTPManager.Name}]");
        sb.AppendLine($"\"{nameof(OtpKeys)}\"=\"{OtpKeys}\"");
        sb.AppendLine();
        return sb.ToString();
    }
}
