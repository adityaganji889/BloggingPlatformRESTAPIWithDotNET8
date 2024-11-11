namespace BloggingPlatform.dtos
{
    public partial class RegisterResponseDto
    {
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Email {get; set;}
        public string Gender {get; set;}
        public string Active {get; set;}
        public string Admin {get; set;}
        public DateTime UserCreated {get; set;}
        public DateTime UserUpdated {get; set;}
        public RegisterResponseDto()
        {
            if (FirstName == null)
            {
                FirstName = "";
            }
            if (LastName == null)
            {
                LastName = "";
            }
            if (Email == null)
            {
                Email = "";
            }
            if (Gender == null)
            {
                Gender = "";
            }
            if (Active == null)
            {
                Active = "";
            }
            if (Admin == null)
            {
                Admin = "";
            }
        }
    }
}