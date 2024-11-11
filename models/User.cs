namespace BloggingPlatform.models {

    
    public partial class User
    {
        public int UserId {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Email {get; set;}
        public byte[] PasswordHash {get; set;}
        public byte[] PasswordSalt {get; set;}
        public bool Gender {get; set;}
        public bool Active {get; set;}
        public bool Admin {get; set;}
        public DateTime UserCreated {get; set;}
        public DateTime UserUpdated {get; set;}

        public User()
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
            if (PasswordHash == null)
            {
                PasswordHash = new byte[0];
            }
            if (PasswordSalt == null)
            {
                PasswordSalt = new byte[0];
            }
        }
    }
}