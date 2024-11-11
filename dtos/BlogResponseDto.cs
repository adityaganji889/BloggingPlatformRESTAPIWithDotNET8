using BloggingPlatform.models;

namespace BloggingPlatform.dtos
{


    public partial class BlogResponseDto
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int AuthorId { get; set; }
        public DateTime BlogCreated { get; set; }
        public DateTime BlogUpdated { get; set; }

        // Navigation property
        public virtual RegisterResponseDto? Author { get; set; }

        public BlogResponseDto()
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