namespace OTPManager.Wpf.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Win32;

public static class AppSettings
{
    public const string RegistryBaseKey = @"SOFTWARE\Illuminati Software Inc.";

    public const string RegistryOTPManagerKey =
#if DEBUG
        RegistryBaseKey + "\\OTPManager [Debug]";
#else
        RegistryBaseKey + "\\OTPManager";
#endif

    public const int CurrentConfigVersion = 1;

    private static readonly RegistryKey RegKeyOTPManager = Registry.CurrentUser.CreateSubKey(RegistryOTPManagerKey);

    private static readonly IReadOnlyDictionary<string, object> DefaultSettingsDict = new Dictionary<string, object>()
    {
        { nameof(ConfigVersion), CurrentConfigVersion },
        { nameof(UpdatesLastCheckedTimestamp), default(DateTime).ToString("s") },
    };

    public static int? ConfigVersion
    {
        get => (int?)RegKeyOTPManager.GetValue(nameof(ConfigVersion));

        set => RegKeyOTPManager.SetValue(nameof(ConfigVersion), value ?? 0);
    }

    public static DateTime UpdatesLastCheckedTimestamp
    {
        get => DateTime.ParseExact((string?)RegKeyOTPManager.GetValue(nameof(UpdatesLastCheckedTimestamp)) ?? string.Empty, "s", CultureInfo.InvariantCulture);

        private set => RegKeyOTPManager.SetValue(nameof(UpdatesLastCheckedTimestamp), value.ToString("s", CultureInfo.InvariantCulture));
    }

    public static void UpdateUpdatesLastCheckedTimestamp() => UpdatesLastCheckedTimestamp = DateTime.Now;

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

    private static void ClearRegistryKey(RegistryKey regKey)
    {
        foreach (string? key in regKey.GetValueNames())
        {
            regKey.DeleteValue(key);
        }
    }
}
