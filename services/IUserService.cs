using BloggingPlatform.models;

namespace BloggingPlatform.services
{
    public interface IUserService
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public IEnumerable<User> GetUsers();
        public User GetSingleUser(int userId);

    }
}