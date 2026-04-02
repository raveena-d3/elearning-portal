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
    public class QuizRepositoryTests
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

            var course = new Course
            {
                Id = 1,
                Title = "Java",
                InstructorId = 1,
                Instructor = instructor
            };

            var quiz1 = new Quiz
            {
                Id = 1,
                Title = "Quiz 1",
                CourseId = 1,
                InstructorId = 1,
                Course = course,
                Instructor = instructor
            };

            var quiz2 = new Quiz
            {
                Id = 2,
                Title = "Quiz 2",
                CourseId = 1,
                InstructorId = 1,
                Course = course,
                Instructor = instructor
            };

            context.Users.Add(instructor);
            context.Courses.Add(course);
            context.Quizzes.AddRange(quiz1, quiz2);

            context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Quiz_With_Details()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result!.Course);
            Assert.NotNull(result.Instructor);
            Assert.Equal("Quiz 1", result.Title);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_All_Quizzes_For_Course()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByCourseAsync(1);

            Assert.Equal(2, result.Count());
            Assert.All(result, q => Assert.NotNull(q.Course));
            Assert.All(result, q => Assert.NotNull(q.Instructor));
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_Empty()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByCourseAsync(99);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_Quizzes()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByInstructorAsync(1);

            Assert.Equal(2, result.Count());
            Assert.All(result, q => Assert.Equal(1, q.InstructorId));
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_Empty()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new QuizRepository(context);

            var result = await repo.GetByInstructorAsync(99);

            Assert.Empty(result);
        }
    }
}