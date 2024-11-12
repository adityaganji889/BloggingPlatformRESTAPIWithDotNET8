using BloggingPlatform.models;

namespace BloggingPlatform.services
{
    public interface IBlogService
    {
        public Task<bool> SaveChanges();
        public Task AddEntity<T>(T entityToAdd);
        public Task RemoveEntity<T>(T entityToAdd);
        public Task<IEnumerable<Blog>> GetBlogs();
        public Task<IEnumerable<Blog>> GetBlogsByAuthor(int authorId);
        public Task<Blog> GetSingleBlog(int blogId);

    }
}