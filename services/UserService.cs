using BloggingPlatform.models;
using BloggingPlatform.repositories;

namespace BloggingPlatform.services
{

    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {

            _userRepository = userRepository;

        }
        
        public bool SaveChanges()
        {
            return _userRepository.SaveChanges();
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _userRepository.AddEntity<T>(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _userRepository.RemoveEntity<T>(entityToAdd);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _userRepository.GetUsers();
            return users;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _userRepository.GetSingleUser(userId);

            if (user != null)
            {
                return user;
            }
            
            throw new Exception("Failed to Get User");
        }

    }
}
