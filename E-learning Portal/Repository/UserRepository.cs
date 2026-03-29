using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ElearningDbContext context) : base(context) { }

        public async Task<User?> GetByUsernameAsync(string username)
            => await _dbSet.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> ExistsAsync(string username)
            => await _dbSet.AnyAsync(u => u.Username == username);

        public async Task<IEnumerable<User>> GetByRoleAsync(Role role)
            => await _dbSet.Where(u => u.Role == role).ToListAsync();
    }
}