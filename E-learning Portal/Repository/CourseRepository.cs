using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ElearningDbContext context) : base(context) { }

        public async Task<Course?> GetByIdWithInstructorAsync(int id)
            => await _dbSet
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Course>> GetAllWithInstructorAsync()
            => await _dbSet
                .Include(c => c.Instructor)
                .ToListAsync();

        public async Task<IEnumerable<Course>> GetByInstructorAsync(int instructorId)
            => await _dbSet
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
    }
}