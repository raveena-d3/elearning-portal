using E_learning_Portal.models;

namespace ElearningAPI.Repositories
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
        Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
    }
}