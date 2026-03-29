using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface IQuizAttemptRepository : IRepository<QuizAttempt>
    {
        Task<IEnumerable<QuizAttempt>> GetByStudentAsync(int studentId);
        Task<IEnumerable<QuizAttempt>> GetByQuizAsync(int quizId);
        Task<QuizAttempt?> GetByStudentAndQuizAsync(int studentId, int quizId);
        Task<QuizAttempt?> GetByIdWithDetailsAsync(int id);
    }
}