namespace BloggingPlatform.dtos
{
    public partial class VerifyEmailRequestDto
    {
        public string Email {get; set;}
        public string OtpValue { get; set; }

        public VerifyEmailRequestDto()
        {
            if (Email == null)
            {
                Email = "";
            }
            if (OtpValue == null)
            {
                OtpValue = "";
            }
        }
    }
}