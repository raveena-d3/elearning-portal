using E_learning_Portal.Dto;

namespace ElearningAPI.Services
{
    public interface IDiscussionService
    {
        Task<DiscussionResponseDTO> AskQuestionAsync(DiscussionCreateDTO dto);
        Task<DiscussionResponseDTO> AnswerQuestionAsync(int discussionId, string answer, int requestingUserId, string requestingUserRole);
        Task<IEnumerable<DiscussionResponseDTO>> GetByCourseAsync(int courseId);
        Task<IEnumerable<DiscussionResponseDTO>> GetByStudentAsync(int studentId);
        Task<IEnumerable<DiscussionResponseDTO>> GetByInstructorAsync(int instructorId);
        Task DeleteAsync(int id, int requestingUserId, string requestingUserRole);
    }
}