using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class DiscussionRepository : Repository<Discussion>, IDiscussionRepository
    {
        public DiscussionRepository(ElearningDbContext context) : base(context) { }

        public async Task<Discussion?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(d => d.Course)
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<IEnumerable<Discussion>> GetByCourseAsync(int courseId)
            => await _dbSet
                .Include(d => d.Course)
                .Include(d => d.Student)
                .Where(d => d.CourseId == courseId)
                .OrderByDescending(d => d.AskedAt)
                .ToListAsync();

        public async Task<IEnumerable<Discussion>> GetByStudentAsync(int studentId)
            => await _dbSet
                .Include(d => d.Course)
                .Include(d => d.Student)
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.AskedAt)
                .ToListAsync();

        public async Task<IEnumerable<Discussion>> GetUnansweredByCourseAsync(int courseId)
            => await _dbSet
                .Include(d => d.Course)
                .Include(d => d.Student)
                .Where(d => d.CourseId == courseId && d.Answer == null)
                .OrderBy(d => d.AskedAt)
                .ToListAsync();
    }
}