using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(ElearningDbContext context) : base(context) { }

        public async Task<Quiz?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .FirstOrDefaultAsync(q => q.Id == id);

        public async Task<IEnumerable<Quiz>> GetByCourseAsync(int courseId)
            => await _dbSet
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

        public async Task<IEnumerable<Quiz>> GetByInstructorAsync(int instructorId)
            => await _dbSet
                .Include(q => q.Course)
                .Include(q => q.Instructor)
                .Where(q => q.InstructorId == instructorId)
                .ToListAsync();
    }
}