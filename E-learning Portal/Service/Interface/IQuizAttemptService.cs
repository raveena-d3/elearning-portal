using E_learning_Portal.Dto;

namespace E_learning_Portal.Service.Interface
{
    public interface IQuizAttemptService
    {
        Task<QuizAttemptResponseDTO> SubmitAttemptAsync(QuizAttemptCreateDTO dto);
        Task<IEnumerable<QuizAttemptResponseDTO>> GetByStudentAsync(int studentId);
        Task<IEnumerable<QuizAttemptResponseDTO>> GetByQuizAsync(int quizId);
        Task<QuizAttemptResponseDTO> GetByIdAsync(int id);
    }
}
