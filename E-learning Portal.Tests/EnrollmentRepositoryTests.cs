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
    public class EnrollmentRepositoryTests
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

            var enrollment = new Enrollment
            {
                Id = 1,
                CourseId = 1,
                StudentId = 2,
                Course = course,
                Student = student,
                EnrolledAt = DateTime.UtcNow
            };

            context.Users.AddRange(instructor, student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_Enrollments()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.GetByStudentAsync(2);

            Assert.Single(result);
            Assert.Equal(2, result.First().StudentId);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_Enrollments()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.GetByCourseAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().CourseId);
        }

        [Fact]
        public async Task GetByStudentAndCourseAsync_Should_Return_Enrollment()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.GetByStudentAndCourseAsync(2, 1);

            Assert.NotNull(result);
            Assert.Equal(2, result!.StudentId);
            Assert.Equal(1, result.CourseId);
        }

        [Fact]
        public async Task GetByStudentAndCourseAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.GetByStudentAndCourseAsync(99, 99);

            Assert.Null(result);
        }

        [Fact]
        public async Task IsEnrolledAsync_Should_Return_True()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.IsEnrolledAsync(2, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task IsEnrolledAsync_Should_Return_False()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new EnrollmentRepository(context);

            var result = await repo.IsEnrolledAsync(2, 99);

            Assert.False(result);
        }
    }
}