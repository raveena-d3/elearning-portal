using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetByIdWithInstructorAsync(int id);
        Task<IEnumerable<Course>> GetAllWithInstructorAsync();
        Task<IEnumerable<Course>> GetByInstructorAsync(int instructorId);
    }
}