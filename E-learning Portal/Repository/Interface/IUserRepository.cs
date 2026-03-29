using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsAsync(string username);
        Task<IEnumerable<User>> GetByRoleAsync(Role role);
    }
}