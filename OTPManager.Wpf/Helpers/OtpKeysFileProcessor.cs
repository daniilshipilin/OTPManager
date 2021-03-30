namespace OTPManager.Wpf.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using OTPManager.Wpf.Models;

    public static class OtpKeysFileProcessor
    {
        private static readonly string OtpFilePath = Path.Combine(Environment.CurrentDirectory, "otpkeys");

        private static byte[] hashedPassword = new byte[32];

        public static bool LoginIsSuccessful { get; private set; }

        public static void SetPassword(string password)
        {
            using var sha256 = SHA256.Create();
            hashedPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool TryReadFile()
        {
            CheckOtpFileExists();

            byte[] encryptedBytes = File.ReadAllBytes(OtpFilePath);
            LoginIsSuccessful = SymmetricEncryptDecrypt.TryDecrypt(encryptedBytes, hashedPassword);

            return LoginIsSuccessful;
        }

        public static bool ChangeFileEncryptionPassword(string currentPassword, string newPassword)
        {
            SetPassword(currentPassword);

            if (TryReadFile())
            {
                var data = LoadData();
                SetPassword(newPassword);
                SaveData(data);
            }

            return LoginIsSuccessful;
        }

        public static IList<OtpObject> LoadData()
        {
            CheckOtpFileExists();

            byte[] encryptedBytes = File.ReadAllBytes(OtpFilePath);
            string json = Encoding.UTF8.GetString(SymmetricEncryptDecrypt.Decrypt(encryptedBytes, hashedPassword));
            var jsonObj = JsonConvert.DeserializeObject<OtpKeysJSON>(json);
            var otps = new List<OtpObject>();

            foreach (var entry in jsonObj.OtpEntries)
            {
                otps.Add(new OtpObject(entry.Description, entry.Base32SecretKey));
            }

            return otps;
        }

        public static void SaveData(IList<OtpObject>? otps)
        {
            var jsonObj = new OtpKeysJSON();

            if (otps is not null)
            {
                foreach (var entry in otps)
                {
                    jsonObj.OtpEntries.Add(new OtpKeysJSON.OtpEntry() { Description = entry.Description, Base32SecretKey = entry.Base32SecretKey });
                }
            }

            byte[] textBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonObj));
            byte[] encryptedBytes = SymmetricEncryptDecrypt.Encrypt(textBytes, hashedPassword);
            File.WriteAllBytes(OtpFilePath, encryptedBytes);
        }

        private static void CheckOtpFileExists()
        {
            var info = new FileInfo(OtpFilePath);

            if (!info.Exists || info.Length == 0)
            {
                // save file with no record entries
                SaveData(null);
            }
        }
    }
}
