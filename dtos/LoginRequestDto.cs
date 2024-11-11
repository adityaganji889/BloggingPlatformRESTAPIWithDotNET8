namespace BloggingPlatform.dtos
{
    public partial class LoginRequestDto
    {
        public string Email {get; set;}
        public string Password {get; set;}
        public LoginRequestDto()
        {
            if (Email == null)
            {
                Email = "";
            }
            if (Password == null)
            {
                Password = "";
            }
        }
    }
}