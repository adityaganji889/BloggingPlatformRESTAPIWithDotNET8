using BloggingPlatform.models;
using BloggingPlatform.repositories;

namespace BloggingPlatform.services
{

    public class BlogService : IBlogService
    {

        private readonly IBlogRepository _blogRepository;
        
        // Constructor with BlogRepository injected
        public BlogService(IBlogRepository blogRepository)
        {

            _blogRepository = blogRepository;

        }
        
        public async Task<bool> SaveChanges()
        {
            return await _blogRepository.SaveChanges();
        }

        public async Task AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _blogRepository.AddEntity<T>(entityToAdd);
            }
        }

        public async Task RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
               await _blogRepository.RemoveEntity<T>(entityToAdd);
            }
        }

        public async Task<IEnumerable<Blog>> GetBlogs()
        {
            IEnumerable<Blog> blogs = await _blogRepository.GetBlogs();
            return blogs;
        }

        public async Task<IEnumerable<Blog>> GetBlogsByAuthor(int authorId)
        {
            IEnumerable<Blog> blogs = await _blogRepository.GetBlogsByAuthor(authorId);
            return blogs;
        }

        public async Task<Blog> GetSingleBlog(int userId)
        {
            Blog? blog = await _blogRepository.GetSingleBlog(userId);

            if (blog != null)
            {
                return blog;
            }
            
            throw new Exception("Failed to Get Blog");
        }

    }
}
