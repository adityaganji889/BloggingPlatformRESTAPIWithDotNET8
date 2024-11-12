using BloggingPlatform.models;

namespace BloggingPlatform.repositories
{
    public interface IUserRepository
    {
        public Task<bool> SaveChanges();
        public Task AddEntity<T>(T entityToAdd);
        public Task RemoveEntity<T>(T entityToAdd);
        public Task<IEnumerable<User>> GetUsers();
        public Task<User> GetSingleUser(int userId);

    }
}