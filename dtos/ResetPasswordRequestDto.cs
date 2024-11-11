namespace BloggingPlatform.dtos
{
    public partial class ResetPasswordRequestDto
    {
        public string Email {get; set;}
        public string Password {get; set;}
        public string PasswordConfirm {get; set;}

        public ResetPasswordRequestDto()
        {
            if (Email == null)
            {
                Email = "";
            }
            if (Password == null)
            {
                Password = "";
            }
            if (PasswordConfirm == null)
            {
                PasswordConfirm = "";
            }
        }
    }
}