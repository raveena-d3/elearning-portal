using E_learning_Portal.Dto;
namespace ElearningAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllAsync();
        Task<UserResponseDTO> GetByIdAsync(int id);
        Task<UserResponseDTO> UpdateRoleAsync(int id, string newRole);
        Task DeleteAsync(int id);
        Task<UserResponseDTO> CreateAsync(CreateUserDTO dto); // ← add this
    }
}