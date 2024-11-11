namespace BloggingPlatform.dtos
{
    public partial class LoginResponseDto
    {
        public string Token {get; set;}
        public LoginResponseDto()
        {
            if(Token == null){
                Token = "";
            }
        }
    }
}