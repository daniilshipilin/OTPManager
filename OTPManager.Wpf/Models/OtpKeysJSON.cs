namespace OTPManager.Wpf.Models;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class OtpKeysJSON
{
    [JsonProperty("filerevision")]
    public static int FileRevision { get; set; }

    [JsonProperty("filelastedittimestamp")]
    public static int FileLastEditTimestamp { get; set; }

    [JsonProperty("otpentries")]
    public IList<OtpEntry> OtpEntries { get; set; } = new List<OtpEntry>();

    public class OtpEntry
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("base32secretkey")]
        public string Base32SecretKey { get; set; } = string.Empty;

        [JsonProperty("lastedittimestamp")]
        public int LastEditTimestamp { get; set; }
    }
}
