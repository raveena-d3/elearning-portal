using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElearningAPI.Data;
using ElearningAPI.Repositories;
using E_learning_Portal.models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class CourseRepositoryTests
    {
        private ElearningDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElearningDbContext(options);
        }

        [Fact]
        public async Task GetByIdWithInstructorAsync_Should_Return_Course_With_Instructor()
        {
            var context = GetDbContext();

            var instructor = new User
            {
                Id = 1,
                Username = "mani",
                Role = Role.Instructor
            };

            var course = new Course
            {
                Id = 1,
                Title = "Java",
                InstructorId = 1,
                Instructor = instructor
            };

            context.Users.Add(instructor);
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var repo = new CourseRepository(context);

            var result = await repo.GetByIdWithInstructorAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Java", result!.Title);
            Assert.NotNull(result.Instructor);
            Assert.Equal("mani", result.Instructor.Username);
        }

        [Fact]
        public async Task GetByIdWithInstructorAsync_Should_Return_Null_When_Not_Found()
        {
            var context = GetDbContext();
            var repo = new CourseRepository(context);

            var result = await repo.GetByIdWithInstructorAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllWithInstructorAsync_Should_Return_All_Courses()
        {
            var context = GetDbContext();

            var instructor = new User
            {
                Id = 1,
                Username = "mani",
                Role = Role.Instructor
            };

            context.Users.Add(instructor);

            context.Courses.AddRange(
                new Course
                {
                    Id = 1,
                    Title = "Java",
                    InstructorId = 1,
                    Instructor = instructor
                },
                new Course
                {
                    Id = 2,
                    Title = "Dotnet",
                    InstructorId = 1,
                    Instructor = instructor
                }
            );

            await context.SaveChangesAsync();

            var repo = new CourseRepository(context);

            var result = await repo.GetAllWithInstructorAsync();

            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.NotNull(c.Instructor));
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_Filtered_Courses()
        {
            var context = GetDbContext();

            var instructor1 = new User
            {
                Id = 1,
                Username = "mani",
                Role = Role.Instructor
            };

            var instructor2 = new User
            {
                Id = 2,
                Username = "trainer",
                Role = Role.Instructor
            };

            context.Users.AddRange(instructor1, instructor2);

            context.Courses.AddRange(
                new Course
                {
                    Id = 1,
                    Title = "Java",
                    InstructorId = 1,
                    Instructor = instructor1
                },
                new Course
                {
                    Id = 2,
                    Title = "Python",
                    InstructorId = 2,
                    Instructor = instructor2
                }
            );

            await context.SaveChangesAsync();

            var repo = new CourseRepository(context);

            var result = await repo.GetByInstructorAsync(1);

            Assert.Single(result);
            Assert.Equal("Java", result.First().Title);
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_Empty_When_No_Courses()
        {
            var context = GetDbContext();
            var repo = new CourseRepository(context);

            var result = await repo.GetByInstructorAsync(99);

            Assert.Empty(result);
        }
    }
}