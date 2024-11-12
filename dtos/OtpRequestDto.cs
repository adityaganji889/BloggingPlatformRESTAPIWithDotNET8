namespace BloggingPlatform.dtos
{

    public class OtpRequestDto
    {
        public string Email { get; set; }

        public OtpRequestDto()
        {
            if (Email == null)
            {
                Email = "";
            }
        }
    }
}