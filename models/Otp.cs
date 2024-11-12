namespace BloggingPlatform.models
{
    public partial class Otp
    {
        public int OtpId { get; set; }
        public int UserId { get; set; }
        public string OtpValue { get; set; }
        public DateTime OtpExpiration { get; set; }
        public DateTime OtpCreated { get; set; }
        public DateTime OtpUpdated { get; set; }

        public virtual User? User { get; set; }
        public Otp()
        {
            if (OtpValue == null)
            {
                OtpValue = "";
            }
        }
    }
}