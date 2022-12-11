namespace OTPManager.Wpf.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class OtpKeysJSON
{
    [JsonPropertyName("filerevision")]
    public static int FileRevision { get; set; }

    [JsonPropertyName("filelastedittimestamp")]
    public static int FileLastEditTimestamp { get; set; }

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
