namespace OTPManager.Wpf.Helpers;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OTPManager.Wpf.Models;

public static class OtpKeysProcessor
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
    private static SymmetricEncryption? encryption;

    public static bool LoginIsSuccessful { get; private set; }

    public static void SetPassword(string password)
    {
        byte[] hashedPassword = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        encryption = new SymmetricEncryption(hashedPassword);
    }

    public static void ResetPassword()
        => encryption?.Dispose();

    public static bool TryParseOtpKeys()
    {
        if (encryption is null)
        {
            throw new ArgumentNullException(nameof(encryption));
        }

        CheckOtpKeysValid();

        byte[] encryptedBytes = Convert.FromBase64String(AppSettings.OtpKeys);
        LoginIsSuccessful = encryption.TryDecrypt(encryptedBytes, out _);

        return LoginIsSuccessful;
    }

    public static bool ChangeEncryptionPassword(string currentPassword, string newPassword)
    {
        SetPassword(currentPassword);

        if (TryParseOtpKeys())
        {
            var data = LoadData();
            SetPassword(newPassword);
            SaveData(data);
        }

        return LoginIsSuccessful;
    }

    public static IEnumerable<OtpObject> LoadData()
    {
        if (encryption is null)
        {
            throw new ArgumentNullException(nameof(encryption));
        }

        CheckOtpKeysValid();

        byte[] encryptedBytes = Convert.FromBase64String(AppSettings.OtpKeys);
        string json = Encoding.UTF8.GetString(encryption.Decrypt(encryptedBytes));
        var jsonObj = JsonSerializer.Deserialize<OtpKeysJson>(json, jsonSerializerOptions);
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
        if (encryption is null)
        {
            throw new ArgumentNullException(nameof(encryption));
        }

        OtpKeysJson.Revision++;
        OtpKeysJson.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();

        string json = GetOtpKeysJson(otps);
        byte[] textBytes = Encoding.UTF8.GetBytes(json);
        byte[] encryptedBytes = encryption.Encrypt(textBytes);
        AppSettings.OtpKeys = Convert.ToBase64String(encryptedBytes);
    }

    public static void SaveData(string json)
    {
        if (encryption is null)
        {
            throw new ArgumentNullException(nameof(encryption));
        }

        var jsonObj = JsonSerializer.Deserialize<OtpKeysJson>(json, jsonSerializerOptions);
        byte[] textBytes = Encoding.UTF8.GetBytes(json);
        byte[] encryptedBytes = encryption.Encrypt(textBytes);
        AppSettings.OtpKeys = Convert.ToBase64String(encryptedBytes);
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

    private static void CheckOtpKeysValid()
    {
        if (string.IsNullOrEmpty(AppSettings.OtpKeys))
        {
            SaveData([]);
        }
    }
}
