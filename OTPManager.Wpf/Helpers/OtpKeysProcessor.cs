namespace OTPManager.Wpf.Helpers;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using OTPManager.Wpf.Models;

public static class OtpKeysProcessor
{
    private static SymmetricEncryption encryption = new SymmetricEncryption();

    public static bool LoginIsSuccessful { get; private set; }

    public static void SetPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] hashedPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(password))[0..32];
        encryption = new SymmetricEncryption(hashedPassword);
    }

    public static void ResetPassword()
    {
        encryption.Dispose();
        encryption = new SymmetricEncryption();
    }

    public static bool TryParseOtpKeys()
    {
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
        CheckOtpKeysValid();

        byte[] encryptedBytes = Convert.FromBase64String(AppSettings.OtpKeys);
        string json = Encoding.UTF8.GetString(encryption.Decrypt(encryptedBytes));
        var jsonObj = JsonConvert.DeserializeObject<OtpKeysJSON>(json);
        var otps = new List<OtpObject>();

        if (jsonObj is not null)
        {
            foreach (var entry in jsonObj.OtpEntries)
            {
                otps.Add(new OtpObject(
                    entry.Description,
                    entry.Base32SecretKey,
                    entry.LastEditTimestamp));
            }
        }

        return otps;
    }

    public static void SaveData(IEnumerable<OtpObject> otps)
    {
        var jsonObj = new OtpKeysJSON();

        if (otps is not null)
        {
            foreach (var entry in otps)
            {
                jsonObj.OtpEntries.Add(new OtpKeysJSON.OtpEntry()
                {
                    Description = entry.Description,
                    Base32SecretKey = entry.Base32SecretKey,
                    LastEditTimestamp = entry.LastEditTimestamp,
                });
            }
        }

        OtpKeysJSON.FileRevision++;
        OtpKeysJSON.FileLastEditTimestamp = TimestampHelper.GetUnixTimestamp();

        byte[] textBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonObj));
        byte[] encryptedBytes = encryption.Encrypt(textBytes);
        AppSettings.OtpKeys = Convert.ToBase64String(encryptedBytes);
    }

    private static void CheckOtpKeysValid()
    {
        if (string.IsNullOrEmpty(AppSettings.OtpKeys))
        {
            // save file with no record entries
            SaveData(new List<OtpObject>());
        }
    }
}
