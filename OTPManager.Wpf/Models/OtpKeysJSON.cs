namespace OTPManager.Wpf.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class OtpKeysJSON
    {
        [JsonProperty("otpentries")]
        public IList<OtpEntry> OtpEntries { get; set; } = new List<OtpEntry>();

        public class OtpEntry
        {
            [JsonProperty("description")]
            public string Description { get; set; } = string.Empty;

            [JsonProperty("base32secretkey")]
            public string Base32SecretKey { get; set; } = string.Empty;

            public override string ToString()
            {
                return $"{nameof(Description)}: {Description},{nameof(Base32SecretKey)}: {Base32SecretKey}";
            }
        }
    }
}
