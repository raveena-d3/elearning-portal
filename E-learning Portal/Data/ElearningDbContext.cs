using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Data
{
    public class ElearningDbContext : DbContext
    {
        public ElearningDbContext(DbContextOptions<ElearningDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Discussion> Discussions
        {
            get
            {
                return Set<Discussion>();
            }
        }
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
       
    }
}