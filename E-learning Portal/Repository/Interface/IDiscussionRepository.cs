using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface IDiscussionRepository : IRepository<Discussion>
    {
        Task<Discussion?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Discussion>> GetByCourseAsync(int courseId);
        Task<IEnumerable<Discussion>> GetByStudentAsync(int studentId);
        Task<IEnumerable<Discussion>> GetUnansweredByCourseAsync(int courseId);
    }
}