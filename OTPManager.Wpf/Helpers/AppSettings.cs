namespace OTPManager.Wpf.Helpers;

using System.Collections.Generic;
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

    public const int CurrentConfigVersion = 3;

    private static readonly RegistryKey RegKeyOTPManager = Registry.CurrentUser.CreateSubKey(RegistryOTPManagerKey);

    private static readonly IReadOnlyDictionary<string, object> DefaultSettingsDict = new Dictionary<string, object>()
    {
        { nameof(ConfigVersion), CurrentConfigVersion },
        { nameof(OtpKeys), string.Empty },
    };

    public static int? ConfigVersion
    {
        get => (int?)RegKeyOTPManager.GetValue(nameof(ConfigVersion));

        set => RegKeyOTPManager.SetValue(nameof(ConfigVersion), value ?? 0);
    }

    public static string OtpKeys
    {
        get => (string?)RegKeyOTPManager.GetValue(nameof(OtpKeys)) ?? string.Empty;

        set => RegKeyOTPManager.SetValue(nameof(OtpKeys), value ?? string.Empty);
    }

    public static void CheckSettings()
    {
        if (ConfigVersion is null or not CurrentConfigVersion)
        {
            ResetSettings();
        }
    }

    public static void ResetSettings()
    {
        // clear root config reg keys
        ClearRegistryKey(RegKeyOTPManager);

        // set default values
        foreach (var pair in DefaultSettingsDict)
        {
            RegKeyOTPManager.SetValue(pair.Key, pair.Value);
        }
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

    private static void ClearRegistryKey(RegistryKey regKey)
    {
        foreach (string? key in regKey.GetValueNames())
        {
            regKey.DeleteValue(key);
        }
    }
}
