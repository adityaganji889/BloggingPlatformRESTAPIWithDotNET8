using System.Runtime.CompilerServices;
using BloggingPlatform.config;
using BloggingPlatform.models;
using Microsoft.EntityFrameworkCore;


namespace BloggingPlatform.repositories
{
    public class UserRepository : IUserRepository
    {
        DataContextEF _entityFramework;    

        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public async Task<bool> SaveChanges()
        {
            return await _entityFramework.SaveChangesAsync() > 0;
        }

        public async Task AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _entityFramework.AddAsync(entityToAdd);
            }
        }

        public async Task RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
               _entityFramework.Remove(entityToAdd);
            }
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            IEnumerable<User> users = await _entityFramework.Users.ToListAsync<User>();
            return users;
        }

        public async Task<User> GetSingleUser(int userId)
        {
            User? user = await _entityFramework.Users
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync<User>();

            if (user != null)
            {
                return user;
            }
            
            throw new Exception("Failed to Get User");
        }

    }
}