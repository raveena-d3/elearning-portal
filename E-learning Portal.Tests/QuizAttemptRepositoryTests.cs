using System;
using System.Linq;
using System.Threading.Tasks;
using ElearningAPI.Data;
using ElearningAPI.Repositories;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class QuizAttemptRepositoryTests
    {
        private ElearningDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElearningDbContext(options);
        }

        private void SeedData(ElearningDbContext context)
        {
            var instructor = new User
            {
                Id = 1,
                Username = "instructor",
                Role = Role.Instructor
            };

            var student = new User
            {
                Id = 2,
                Username = "student",
                Role = Role.Student
            };

            var course = new Course
            {
                Id = 1,
                Title = "Java",
                InstructorId = 1,
                Instructor = instructor
            };

            var quiz = new Quiz
            {
                Id = 1,
                Title = "Quiz 1",
                CourseId = 1,
                InstructorId = 1,
                Course = course,
                Instructor = instructor
            };

            var attempt = new QuizAttempt
            {
                Id = 1,
                QuizId = 1,
                Quiz = quiz,
                StudentId = 2,
                Student = student,
                Score = 80,
                AttemptedAt = DateTime.UtcNow
            };

            context.Users.AddRange(instructor, student);
            context.Courses.Add(course);
            context.Quizzes.Add(quiz);
            context.QuizAttempts.Add(attempt);

            context.SaveChanges();
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_Attempts()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByStudentAsync(2);

            Assert.Single(result);
            Assert.Equal(2, result.First().StudentId);
            Assert.NotNull(result.First().Quiz);
            Assert.NotNull(result.First().Quiz.Course);
        }

        [Fact]
        public async Task GetByQuizAsync_Should_Return_Attempts()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByQuizAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().QuizId);
        }

        [Fact]
        public async Task GetByStudentAndQuizAsync_Should_Return_Attempt()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByStudentAndQuizAsync(2, 1);

            Assert.NotNull(result);
            Assert.Equal(2, result!.StudentId);
            Assert.Equal(1, result.QuizId);
        }

        [Fact]
        public async Task GetByStudentAndQuizAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByStudentAndQuizAsync(99, 99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Attempt_With_Details()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result!.Quiz);
            Assert.NotNull(result.Quiz.Course);
            Assert.NotNull(result.Student);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizAttemptRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(999);

            Assert.Null(result);
        }
    }
}