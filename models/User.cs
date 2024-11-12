using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.models {

    
    public partial class User
    {
        public int UserId {get; set;}

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName {get; set;}

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName {get; set;}

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email {get; set;}

        [Required(ErrorMessage = "Password hash is required.")]
        public byte[] PasswordHash {get; set;}

        [Required(ErrorMessage = "Password salt is required.")]
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