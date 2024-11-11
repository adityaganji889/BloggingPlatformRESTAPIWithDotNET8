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
        
        public bool SaveChanges()
        {
            return _blogRepository.SaveChanges();
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _blogRepository.AddEntity<T>(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _blogRepository.RemoveEntity<T>(entityToAdd);
            }
        }

        public IEnumerable<Blog> GetBlogs()
        {
            IEnumerable<Blog> blogs = _blogRepository.GetBlogs();
            return blogs;
        }

        public IEnumerable<Blog> GetBlogsByAuthor(int authorId)
        {
            IEnumerable<Blog> blogs = _blogRepository.GetBlogsByAuthor(authorId);
            return blogs;
        }

        public Blog GetSingleBlog(int userId)
        {
            Blog? blog = _blogRepository.GetSingleBlog(userId);

            if (blog != null)
            {
                return blog;
            }
            
            throw new Exception("Failed to Get Blog");
        }

    }
}
