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
    public class DiscussionRepositoryTests
    {
        private ElearningDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElearningDbContext(options);
        }

        private Discussion SeedData(ElearningDbContext context)
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

            var discussion = new Discussion
            {
                Id = 1,
                CourseId = 1,
                Course = course,
                StudentId = 2,
                Student = student,
                Question = "What is JVM?",
                AskedAt = DateTime.UtcNow
            };

            context.Users.AddRange(instructor, student);
            context.Courses.Add(course);
            context.Discussions.Add(discussion);
            context.SaveChanges();

            return discussion;
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Discussion()
        {
            var context = GetDbContext();
            var discussion = SeedData(context);

            var repo = new DiscussionRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(discussion.Id);

            Assert.NotNull(result);
            Assert.NotNull(result!.Course);
            Assert.NotNull(result.Student);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            var repo = new DiscussionRepository(context);

            var result = await repo.GetByIdWithDetailsAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_Filtered_Discussions()
        {
            var context = GetDbContext();
            var discussion = SeedData(context);

            var repo = new DiscussionRepository(context);

            var result = await repo.GetByCourseAsync(1);

            Assert.Single(result);
            Assert.Equal("What is JVM?", result.First().Question);
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_Filtered_Discussions()
        {
            var context = GetDbContext();
            var discussion = SeedData(context);

            var repo = new DiscussionRepository(context);

            var result = await repo.GetByStudentAsync(2);

            Assert.Single(result);
            Assert.Equal("What is JVM?", result.First().Question);
        }

        [Fact]
        public async Task GetUnansweredByCourseAsync_Should_Return_Only_Unanswered()
        {
            var context = GetDbContext();
            var discussion = SeedData(context);

            var answered = new Discussion
            {
                Id = 2,
                CourseId = 1,
                StudentId = 2,
                Question = "Answered Q",
                Answer = "Answer",
                AskedAt = DateTime.UtcNow
            };

            context.Discussions.Add(answered);
            await context.SaveChangesAsync();

            var repo = new DiscussionRepository(context);

            var result = await repo.GetUnansweredByCourseAsync(1);

            Assert.Single(result);
            Assert.Null(result.First().Answer);
        }

        [Fact]
        public async Task GetUnansweredByCourseAsync_Should_Return_Empty()
        {
            var context = GetDbContext();

            var repo = new DiscussionRepository(context);

            var result = await repo.GetUnansweredByCourseAsync(1);

            Assert.Empty(result);
        }
    }
}