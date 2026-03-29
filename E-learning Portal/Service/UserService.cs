using ElearningAPI.Repositories;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;

namespace ElearningAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => u.ToDTO());
        }

        public async Task<UserResponseDTO> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new Exception("User not found.");
            return user.ToDTO();
        }

        public async Task<UserResponseDTO> CreateAsync(CreateUserDTO dto)
        {
            // Check if username already exists
            var existing = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existing != null)
                throw new Exception("Username already exists.");

            if (!Enum.TryParse<Role>(dto.Role, true, out var parsedRole))
                throw new Exception("Invalid role.");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = "keycloak-managed",
                Role = parsedRole
            };

            await _userRepository.AddAsync(user);
            return user.ToDTO();
        }

        public async Task<UserResponseDTO> UpdateRoleAsync(int id, string newRole)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new Exception("User not found.");

            if (!Enum.TryParse<Role>(newRole, true, out var parsedRole))
                throw new Exception("Invalid role. Use Admin, Instructor, or Student.");

            user.Role = parsedRole;
            await _userRepository.UpdateAsync(user);
            return user.ToDTO();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new Exception("User not found.");
            await _userRepository.DeleteAsync(user);
        }
    }
}