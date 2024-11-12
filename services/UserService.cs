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
        
        public async Task<bool> SaveChanges()
        {
            return await _userRepository.SaveChanges();
        }

        public async Task AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _userRepository.AddEntity<T>(entityToAdd);
            }
        }

        public async Task RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                await _userRepository.RemoveEntity<T>(entityToAdd);
            }
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            IEnumerable<User> users = await _userRepository.GetUsers();
            return users;
        }

        public async Task<User> GetSingleUser(int userId)
        {
            User? user = await _userRepository.GetSingleUser(userId);

            if (user != null)
            {
                return user;
            }
            
            throw new Exception("Failed to Get User");
        }

    }
}
