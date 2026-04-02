using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElearningAPI.Repositories;
using ElearningAPI.Services;
using E_learning_Portal.Dto;
using E_learning_Portal.models;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class EnrollmentServiceTests
    {
        private readonly Mock<IEnrollmentRepository> _enrollmentRepo;
        private readonly Mock<ICourseRepository> _courseRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly EnrollmentService _service;

        public EnrollmentServiceTests()
        {
            _enrollmentRepo = new Mock<IEnrollmentRepository>();
            _courseRepo = new Mock<ICourseRepository>();
            _userRepo = new Mock<IUserRepository>();

            _service = new EnrollmentService(
                _enrollmentRepo.Object,
                _courseRepo.Object,
                _userRepo.Object
            );
        }

        [Fact]
        public async Task EnrollAsync_Should_Success()
        {
            var dto = new EnrollmentCreateDTO
            {
                CourseId = 1,
                StudentId = 10
            };

            var course = new Course
            {
                Id = 1,
                Title = "Java"
            };

            var student = new User
            {
                Id = 10,
                Username = "student",
                Role = Role.Student
            };

            _courseRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(course);

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(student);

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(false);

            _enrollmentRepo.Setup(x => x.AddAsync(It.IsAny<Enrollment>()))
                .ReturnsAsync((Enrollment e) => e);

            var result = await _service.EnrollAsync(dto);

            Assert.Equal(1, result.CourseId);
            Assert.Equal(10, result.StudentId);
            Assert.Equal("Java", result.CourseTitle);
            Assert.Equal("student", result.StudentName);
        }

        [Fact]
        public async Task EnrollAsync_Should_Throw_When_Course_Not_Found()
        {
            _courseRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Course?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.EnrollAsync(new EnrollmentCreateDTO
                {
                    CourseId = 1,
                    StudentId = 10
                }));

            Assert.Equal("Course not found.", ex.Message);
        }

        [Fact]
        public async Task EnrollAsync_Should_Throw_When_Student_Not_Found()
        {
            _courseRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Course { Id = 1 });

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.EnrollAsync(new EnrollmentCreateDTO
                {
                    CourseId = 1,
                    StudentId = 10
                }));

            Assert.Equal("Student not found.", ex.Message);
        }

        [Fact]
        public async Task EnrollAsync_Should_Throw_When_Not_Student()
        {
            _courseRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Course { Id = 1 });

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Role = Role.Admin
                });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.EnrollAsync(new EnrollmentCreateDTO
                {
                    CourseId = 1,
                    StudentId = 10
                }));

            Assert.Equal("Only Students can enroll in courses.", ex.Message);
        }

        [Fact]
        public async Task EnrollAsync_Should_Throw_When_Already_Enrolled()
        {
            _courseRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Course { Id = 1 });

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Role = Role.Student
                });

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.EnrollAsync(new EnrollmentCreateDTO
                {
                    CourseId = 1,
                    StudentId = 10
                }));

            Assert.Equal("Student is already enrolled in this course.", ex.Message);
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_List()
        {
            var list = new List<Enrollment>
            {
                new Enrollment
                {
                    Id = 1,
                    CourseId = 1,
                    StudentId = 10,
                    Course = new Course { Title = "Java" },
                    Student = new User { Username = "student" }
                }
            };

            _enrollmentRepo.Setup(x => x.GetByStudentAsync(10))
                .ReturnsAsync(list);

            var result = await _service.GetByStudentAsync(10);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_List()
        {
            var list = new List<Enrollment>
            {
                new Enrollment
                {
                    Id = 1,
                    CourseId = 1,
                    StudentId = 10,
                    Course = new Course { Title = "Java" },
                    Student = new User { Username = "student" }
                }
            };

            _enrollmentRepo.Setup(x => x.GetByCourseAsync(1))
                .ReturnsAsync(list);

            var result = await _service.GetByCourseAsync(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task UnenrollAsync_Should_Delete_When_Admin()
        {
            var enrollment = new Enrollment
            {
                Id = 1,
                StudentId = 10
            };

            _enrollmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(enrollment);

            _enrollmentRepo.Setup(x => x.DeleteAsync(enrollment))
                .Returns(Task.CompletedTask);

            await _service.UnenrollAsync(1, 99, "Admin");

            _enrollmentRepo.Verify(x => x.DeleteAsync(enrollment), Times.Once);
        }

        [Fact]
        public async Task UnenrollAsync_Should_Throw_When_Not_Found()
        {
            _enrollmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Enrollment?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UnenrollAsync(1, 99, "Admin"));

            Assert.Equal("Enrollment not found.", ex.Message);
        }

        [Fact]
        public async Task UnenrollAsync_Should_Throw_When_Not_Owner()
        {
            var enrollment = new Enrollment
            {
                Id = 1,
                StudentId = 20
            };

            _enrollmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(enrollment);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UnenrollAsync(1, 10, "Student"));

            Assert.Equal("You can only unenroll yourself.", ex.Message);
        }

        [Fact]
        public async Task IsEnrolledAsync_Should_Return_True()
        {
            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            var result = await _service.IsEnrolledAsync(10, 1);

            Assert.True(result);
        }
    }
}