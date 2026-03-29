using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(ElearningDbContext context) : base(context) { }

        public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
            => await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId)
            => await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

        public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId)
            => await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
            => await _dbSet.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }
}