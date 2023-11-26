namespace OTPManager.Wpf.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class OtpKeysJSON
{
    public static int Revision { get; set; }

    public static int LastEditTimestamp { get; set; }

    [JsonPropertyName("filerevision")]
#pragma warning disable CA1822 // Mark members as static
    public int FileRevision
#pragma warning restore CA1822 // Mark members as static
    {
        get => Revision;
        set => Revision = value;
    }

    [JsonPropertyName("filelastedittimestamp")]
#pragma warning disable CA1822 // Mark members as static
    public int FileLastEditTimestamp
#pragma warning restore CA1822 // Mark members as static
    {
        get => LastEditTimestamp;
        set => LastEditTimestamp = value;
    }

    [JsonPropertyName("otpentries")]
    public IList<OtpEntry> OtpEntries { get; set; } = new List<OtpEntry>();

    public class OtpEntry
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("base32secretkey")]
        public string Base32SecretKey { get; set; } = string.Empty;

        [JsonPropertyName("lastedittimestamp")]
        public int LastEditTimestamp { get; set; }
    }
}
