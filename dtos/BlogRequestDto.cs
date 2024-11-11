namespace BloggingPlatform.dtos {

    
    public partial class BlogRequestDto
    {
        public string BlogTitle {get; set;}
        public string BlogContent {get; set;}


        public BlogRequestDto()
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