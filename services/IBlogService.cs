using BloggingPlatform.models;

namespace BloggingPlatform.services
{
    public interface IBlogService
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public IEnumerable<Blog> GetBlogs();
        public IEnumerable<Blog> GetBlogsByAuthor(int authorId);
        public Blog GetSingleBlog(int blogId);

    }
}