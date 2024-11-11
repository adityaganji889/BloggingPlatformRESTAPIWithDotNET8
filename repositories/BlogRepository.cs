using BloggingPlatform.config;
using BloggingPlatform.models;
using Microsoft.EntityFrameworkCore;


namespace BloggingPlatform.repositories
{
    public class BlogRepository : IBlogRepository
    {
        DataContextEF _entityFramework;

        public BlogRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
                // return true;
            }
            // return false;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public void RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Remove(entityToAdd);
                // return true;
            }
            // return false;
        }

        public IEnumerable<Blog> GetBlogs()
        {
            IEnumerable<Blog> blogs = _entityFramework.Blogs.Include(b => b.Author).ToList<Blog>();
            return blogs;
        }

        public IEnumerable<Blog> GetBlogsByAuthor(int authorId)
        {
            IEnumerable<Blog> blogs = _entityFramework.Blogs.Where(u => u.AuthorId == authorId).Include(b => b.Author).ToList<Blog>();
            return blogs;
        }

        public Blog GetSingleBlog(int blogId)
        {
            Blog? blog = _entityFramework.Blogs
                .Where(u => u.BlogId == blogId)
                .Include(b => b.Author)
                .FirstOrDefault<Blog>();

            if (blog != null)
            {
                return blog;
            }

            throw new Exception("Failed to Get Blog");
        }

    }
}