namespace OTPManager.Wpf.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class OtpKeysJSON
{
    public static int Revision { get; set; }

    public static int LastEditTimestamp { get; set; }

    [JsonPropertyName("filerevision")]
    public static int FileRevision
    {
        get => Revision;
        set => Revision = value;
    }

    [JsonPropertyName("filelastedittimestamp")]
    public static int FileLastEditTimestamp
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
