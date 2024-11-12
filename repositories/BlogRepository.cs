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

        public async Task<bool> SaveChanges()
        {
            return await _entityFramework.SaveChangesAsync() > 0;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public async Task AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _entityFramework.AddAsync(entityToAdd);
                // return true;
            }
            // return false;
        }

        // public bool AddEntity<T>(T entityToAdd)
        public async Task RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Remove(entityToAdd);
                // return true;
            }
            // return false;
        }

        public async Task<IEnumerable<Blog>> GetBlogs()
        {
            IEnumerable<Blog> blogs = await _entityFramework.Blogs.Include(b => b.Author).ToListAsync<Blog>();
            return blogs;
        }

        public async Task<IEnumerable<Blog>> GetBlogsByAuthor(int authorId)
        {
            IEnumerable<Blog> blogs = await _entityFramework.Blogs.Where(u => u.AuthorId == authorId).Include(b => b.Author).ToListAsync<Blog>();
            return blogs;
        }

        public async Task<Blog> GetSingleBlog(int blogId)
        {
            Blog? blog = await _entityFramework.Blogs
                .Where(u => u.BlogId == blogId)
                .Include(b => b.Author)
                .FirstOrDefaultAsync<Blog>();

            if (blog != null)
            {
                return blog;
            }

            throw new Exception("Failed to Get Blog");
        }

    }
}