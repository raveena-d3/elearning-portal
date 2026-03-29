using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<Quiz?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Quiz>> GetByCourseAsync(int courseId);
        Task<IEnumerable<Quiz>> GetByInstructorAsync(int instructorId);
    }
}