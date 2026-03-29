using ElearningAPI.Data;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Repositories
{
    public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(ElearningDbContext context) : base(context) { }

        public async Task<IEnumerable<QuizAttempt>> GetByStudentAsync(int studentId)
            => await _dbSet
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(a => a.Student)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AttemptedAt)
                .ToListAsync();

        public async Task<IEnumerable<QuizAttempt>> GetByQuizAsync(int quizId)
            => await _dbSet
                .Include(a => a.Quiz)
                .Include(a => a.Student)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.AttemptedAt)
                .ToListAsync();

        public async Task<QuizAttempt?> GetByStudentAndQuizAsync(int studentId, int quizId)
            => await _dbSet
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.QuizId == quizId);

        public async Task<QuizAttempt?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Course)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);
    }
}
