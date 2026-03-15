namespace OTPManager.Wpf.Helpers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using OTPManager.Wpf.Models;

public static class OtpKeysProcessor
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
    private static CipherService encryption = new([]);

    public static bool LoginIsSuccessful { get; private set; }

    public static void SetPassword(byte[] password)
        => encryption = new CipherService(password);

    public static bool TryParseOtpKeys()
    {
        if (string.IsNullOrEmpty(AppSettings.OtpKeys))
        {
            SaveData([]);
        }

        byte[] cipherText = Convert.FromBase64String(AppSettings.OtpKeys);
        LoginIsSuccessful = encryption.TryDecrypt(cipherText, out string? plainText);

        return LoginIsSuccessful;
    }

    public static IEnumerable<OtpObject> LoadData()
    {
        byte[] cipherText = Convert.FromBase64String(AppSettings.OtpKeys);

        if (!encryption.TryDecrypt(cipherText, out string? json))
        {
            return [];
        }

        var jsonObj = JsonSerializer.Deserialize<OtpKeysJson>(json!, jsonSerializerOptions);
        var otps = new List<OtpObject>();

        if (jsonObj is not null)
        {
            foreach (var entry in jsonObj.OtpEntries)
            {
                otps.Add(new OtpObject(
                    entry.Id,
                    entry.Description,
                    entry.Base32SecretKey,
                    entry.IsFavorite,
                    entry.LastEditTimestamp));
            }
        }

        return otps;
    }

    public static void SaveData(IEnumerable<OtpObject> otps)
    {
        OtpKeysJson.Revision++;
        OtpKeysJson.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();

        string json = GetOtpKeysJson(otps);
        byte[] cipherText = encryption.Encrypt(json);
        AppSettings.OtpKeys = Convert.ToBase64String(cipherText, Base64FormattingOptions.InsertLineBreaks);
    }

    public static void SaveData(string json)
    {
        JsonSerializer.Deserialize<OtpKeysJson>(json, jsonSerializerOptions);
        byte[] cipherText = encryption.Encrypt(json);
        AppSettings.OtpKeys = Convert.ToBase64String(cipherText, Base64FormattingOptions.InsertLineBreaks);
    }

    public static string GetOtpKeysJson(IEnumerable<OtpObject> otps)
    {
        var jsonObj = new OtpKeysJson();

        if (otps is not null)
        {
            foreach (var entry in otps)
            {
                jsonObj.OtpEntries.Add(new OtpKeysJson.OtpEntry
                {
                    Id = entry.Id == Guid.Empty ? Guid.CreateVersion7() : entry.Id,
                    Description = entry.Description,
                    Base32SecretKey = entry.Base32SecretKey,
                    IsFavorite = entry.IsFavorite,
                    LastEditTimestamp = entry.LastEditTimestamp,
                });
            }
        }

        return JsonSerializer.Serialize(jsonObj, jsonSerializerOptions);
    }
}
