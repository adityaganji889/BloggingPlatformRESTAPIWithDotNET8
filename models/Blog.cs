using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.models
{


    public partial class Blog
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Blog title is required.")]
        public string BlogTitle { get; set; }

        [Required(ErrorMessage = "Blog content is required.")]
        public string BlogContent { get; set; }

        [Required(ErrorMessage = "Author ID is required.")]
        public int AuthorId { get; set; }
        public DateTime BlogCreated { get; set; }
        public DateTime BlogUpdated { get; set; }

        // Navigation property
        public virtual User? Author { get; set; }

        public Blog()
        {
            if (BlogTitle == null)
            {
                BlogTitle = "";
            }
            if (BlogContent == null)
            {
                BlogContent = "";
            }
        }
    }
}