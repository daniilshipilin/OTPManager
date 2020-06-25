using System;
using OtpNet;

namespace OTPManager.Models
{
    public class OtpObject
    {
        readonly Totp _totp;
        private string _base32SecretKey;

        public int TimeWindowStep { get; } = 30;
        public OtpHashMode HashMode { get; } = OtpHashMode.Sha1;
        public int TotpSize { get; } = 6;
        public TimeCorrection TimeCorr { get; } = null;
        public int ID { get; }
        public string Description { get; set; }
        public string Base32SecretKey
        {
            get { return _base32SecretKey.ToUpper(); }
            set { _base32SecretKey = value.ToUpper(); }
        }

        public OtpObject(int id, string description, string base32SecretKey)
        {
            ID = id;
            Description = description;
            Base32SecretKey = base32SecretKey;
            _totp = new Totp(Base32Encoding.ToBytes(base32SecretKey), TimeWindowStep, HashMode, TotpSize, TimeCorr);
        }

        public int RemainingSeconds => _totp.RemainingSeconds();

        public string TotpValue => _totp.ComputeTotp();

        public static bool Base32SecretKeyIsValid(string base32SecretKey)
        {
            try
            {
                _ = Base32Encoding.ToBytes(base32SecretKey);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
