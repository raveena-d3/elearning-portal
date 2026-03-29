using E_learning_Portal.Dto;

namespace ElearningAPI.Services
{
    public interface IQuizService
    {
        Task<QuizResponseDTO> CreateAsync(QuizCreateDTO dto, int requestingUserId, string requestingUserRole);
        Task<IEnumerable<QuizResponseDTO>> GetByCourseAsync(int courseId);
        Task<QuizResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<QuizResponseDTO>> GetByInstructorAsync(int instructorId);
        Task DeleteAsync(int id, int requestingUserId, string requestingUserRole);
    }
}