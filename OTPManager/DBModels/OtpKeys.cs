namespace OTPManager.DBModels
{
    public class OtpKeys
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Base32SecretKey { get; set; }
    }
}
