namespace OTPManager.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using OTPManager.Wpf.Helpers;
using OtpNet;

public class OtpObject
{
    public static readonly IReadOnlyList<char> Base32Charset = new char[32]
    {
        '2', '3', '4', '5', '6', '7',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
    };

    private Totp? totp;
    private string? base32SecretKey;

    public OtpObject(string description, string base32SecretKey, bool isFavorite)
    {
        this.Description = description;
        this.Base32SecretKey = base32SecretKey;
        this.IsFavorite = isFavorite;
        this.LastEditTimestamp = TimestampHelper.GetUnixTimestamp();
    }

    public OtpObject(string description, string base32SecretKey, bool isFavorite, int lastEditTimestamp)
    {
        this.Description = description;
        this.Base32SecretKey = base32SecretKey;
        this.IsFavorite = isFavorite;
        this.LastEditTimestamp = lastEditTimestamp;
    }

    public string Description { get; set; }

    public string Base32SecretKey
    {
        get => this.base32SecretKey ?? string.Empty;

        set
        {
            string tmp = value.ToUpper().Trim()
            .Replace(" ", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty);

            if (!Base32SecretKeyIsValid(tmp))
            {
                // invalid char detected - don't assign value - throw exception
                throw new ArgumentException(nameof(this.Base32SecretKey));
            }

            this.base32SecretKey = tmp;
            this.totp = new Totp(
                Base32Encoding.ToBytes(this.base32SecretKey),
                this.TimeWindowStep,
                this.HashMode,
                this.TotpSize,
                this.TimeCorr);
        }
    }

    public bool IsFavorite { get; set; }

    public int LastEditTimestamp { get; set; }

    public string LastEditTimestampFormatted => TimestampHelper.UnixTimeStampToDateTime(this.LastEditTimestamp).ToString("s");

    public int TimeWindowStep { get; } = 30;

    public OtpHashMode HashMode { get; } = OtpHashMode.Sha1;

    public int TotpSize { get; } = 6;

    public TimeCorrection? TimeCorr { get; }

    public int RemainingSeconds => this.totp?.RemainingSeconds() ?? 0;

    public string TotpValue => this.totp?.ComputeTotp() ?? string.Empty;

    public static OtpObject GetRandomOtpObject()
        => new OtpObject("_NewOtpKey", GetRandomBase32String(), false);

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
        foreach (char item in base32SecretKey)
        {
            if (!Base32Charset.Contains(item))
            {
                return false;
            }
        }

        return true;
    }
}
