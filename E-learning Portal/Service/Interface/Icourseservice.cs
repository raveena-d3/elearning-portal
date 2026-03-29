using E_learning_Portal.Dto;

namespace ElearningAPI.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponseDTO>> GetAllAsync();
        Task<CourseResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<CourseResponseDTO>> GetByInstructorAsync(int instructorId);
        Task<CourseResponseDTO> CreateAsync(CourseCreateDTO dto);
        Task<CourseResponseDTO> UpdateAsync(int id, CourseCreateDTO dto, int requestingUserId, string requestingUserRole);
        Task<CourseResponseDTO> UpdateVideoAsync(int id, string fileName, string originalName,int userId, string role);
        Task DeleteAsync(int id, int requestingUserId, string requestingUserRole);
        Task ClearVideoAsync(int id);

    }
}