namespace BloggingPlatform.models {

    
    public partial class Blog
    {
        public int BlogId {get; set;}
        public string BlogTitle {get; set;}
        public string BlogContent {get; set;}
        public int AuthorId  {get; set;}
        public DateTime BlogCreated {get; set;}
        public DateTime BlogUpdated {get; set;}

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