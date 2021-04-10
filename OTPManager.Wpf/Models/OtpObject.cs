namespace OTPManager.Wpf.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using OtpNet;

    public class OtpObject
    {
        public static readonly IReadOnlyList<char> Base32Charset = new char[]
        {
            '2', '3', '4', '5', '6', '7',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        };

        private Totp? totp;
        private string? base32SecretKey;

        public OtpObject(string description, string base32SecretKey)
        {
            Description = description;
            Base32SecretKey = base32SecretKey;
        }

        public string Description { get; set; }

        public string Base32SecretKey
        {
            get => base32SecretKey ?? string.Empty;

            set
            {
                string tmp = value.ToUpper().Replace(" ", string.Empty);

                if (!Base32SecretKeyIsValid(tmp))
                {
                    // invalid char detected - don't assign value - throw exception
                    throw new ArgumentException(nameof(Base32SecretKey));
                }

                base32SecretKey = tmp;
                totp = new Totp(Base32Encoding.ToBytes(Base32SecretKey), TimeWindowStep, HashMode, TotpSize, TimeCorr);
            }
        }

        public int TimeWindowStep { get; } = 30;

        public OtpHashMode HashMode { get; } = OtpHashMode.Sha1;

        public int TotpSize { get; } = 6;

        public TimeCorrection? TimeCorr { get; }

        public int RemainingSeconds => totp?.RemainingSeconds() ?? 0;

        public string TotpValue => totp?.ComputeTotp() ?? string.Empty;

        public static OtpObject GetRandomOtpObject()
        {
            return new OtpObject("Otp Key", GetRandomBase32String());
        }

        public static string GetRandomBase32String(int length = 32)
        {
            char[] result = new char[length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Base32Charset[RandomNumberGenerator.GetInt32(Base32Charset.Count)];
            }

            return new string(result);
        }

        private static bool Base32SecretKeyIsValid(string base32SecretKey)
        {
            foreach (var item in base32SecretKey)
            {
                if (!Base32Charset.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
