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
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository> _courseRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly CourseService _service;

        public CourseServiceTests()
        {
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new CourseService(_courseRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Courses_As_DTOs()
        {
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    Title = "Java",
                    Description = "Java Basics",
                    InstructorId = 2,
                    Instructor = new User { Id = 2, Username = "mani", Role = Role.Instructor }
                },
                new Course
                {
                    Id = 2,
                    Title = "ASP.NET",
                    Description = "Dotnet Basics",
                    InstructorId = 3,
                    Instructor = new User { Id = 3, Username = "trainer", Role = Role.Instructor }
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetAllWithInstructorAsync())
                .ReturnsAsync(courses);

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Equal("Java", result.First().Title);
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_Filtered_Courses_As_DTOs()
        {
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    Title = "Java",
                    Description = "Java Basics",
                    InstructorId = 2,
                    Instructor = new User { Id = 2, Username = "mani", Role = Role.Instructor }
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByInstructorAsync(2))
                .ReturnsAsync(courses);

            var result = await _service.GetByInstructorAsync(2);

            var dto = Assert.Single(result);
            Assert.Equal(2, dto.InstructorId);
            Assert.Equal("mani", dto.InstructorName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_CourseDTO_When_Course_Exists()
        {
            var course = new Course
            {
                Id = 1,
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Java", result.Title);
            Assert.Equal("mani", result.InstructorName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_Exception_When_Course_Not_Found()
        {
            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(99))
                .ReturnsAsync((Course?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(99));
            Assert.Equal("Course not found.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Course_When_Instructor_Is_Valid()
        {
            var dto = new CourseCreateDTO
            {
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 2,
                YoutubeLinks = new List<string> { "https://youtu.be/a1" }
            };

            var instructor = new User
            {
                Id = 2,
                Username = "mani",
                Role = Role.Instructor
            };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(instructor);

            _courseRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Course>()))
                .ReturnsAsync((Course c) => c);

            var result = await _service.CreateAsync(dto);

            Assert.Equal("Java", result.Title);
            Assert.Equal(2, result.InstructorId);
            Assert.Equal("mani", result.InstructorName);

            _courseRepositoryMock.Verify(r => r.AddAsync(It.Is<Course>(c =>
                c.Title == "Java" &&
                c.Description == "Java Basics" &&
                c.InstructorId == 2 &&
                c.Instructor.Username == "mani")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_Exception_When_Instructor_Not_Found()
        {
            var dto = new CourseCreateDTO
            {
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 99
            };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
            Assert.Equal("Instructor not found.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_Exception_When_User_Is_Not_Instructor_Or_Admin()
        {
            var dto = new CourseCreateDTO
            {
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 5
            };

            var student = new User
            {
                Id = 5,
                Username = "student1",
                Role = Role.Student
            };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(student);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
            Assert.Equal("Only Instructors or Admins can be assigned as course instructor.", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Course_When_Admin_Or_Owner_Instructor()
        {
            var dto = new CourseCreateDTO
            {
                Title = "Updated Java",
                Description = "Updated Description",
                YoutubeLinks = new List<string> { "https://youtu.be/new1" }
            };

            var course = new Course
            {
                Id = 1,
                Title = "Old Java",
                Description = "Old Description",
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            _courseRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(1, dto, 2, "Instructor");

            Assert.Equal("Updated Java", result.Title);
            Assert.Equal("Updated Description", result.Description);

            _courseRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Course>(c =>
                c.Title == "Updated Java" &&
                c.Description == "Updated Description")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_UnauthorizedAccessException_When_Instructor_Tries_To_Update_Other_Course()
        {
            var dto = new CourseCreateDTO
            {
                Title = "Updated Java",
                Description = "Updated Description"
            };

            var course = new Course
            {
                Id = 1,
                Title = "Old Java",
                Description = "Old Description",
                InstructorId = 10,
                Instructor = new User
                {
                    Id = 10,
                    Username = "otherInstructor",
                    Role = Role.Instructor
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateAsync(1, dto, 2, "Instructor"));

            Assert.Equal("You can only update your own courses.", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Course_When_Admin()
        {
            var course = new Course
            {
                Id = 1,
                InstructorId = 2
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(course);

            _courseRepositoryMock
                .Setup(r => r.DeleteAsync(course))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(1, 99, "Admin");

            _courseRepositoryMock.Verify(r => r.DeleteAsync(course), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_UnauthorizedAccessException_When_Instructor_Tries_To_Delete_Other_Course()
        {
            var course = new Course
            {
                Id = 1,
                InstructorId = 10
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(course);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteAsync(1, 2, "Instructor"));

            Assert.Equal("You can only delete your own courses.", ex.Message);
        }

        [Fact]
        public async Task UpdateVideoAsync_Should_Update_Video_Details_When_Authorized()
        {
            var course = new Course
            {
                Id = 1,
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            _courseRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateVideoAsync(1, "video.mp4", "original.mp4", 2, "Instructor");

            Assert.Equal("video.mp4", result.VideoFileName);
            Assert.Equal("original.mp4", result.VideoOriginalName);
        }

        [Fact]
        public async Task UpdateVideoAsync_Should_Throw_UnauthorizedAccessException_When_Instructor_Tries_To_Update_Other_Course_Video()
        {
            var course = new Course
            {
                Id = 1,
                InstructorId = 10,
                Instructor = new User
                {
                    Id = 10,
                    Username = "otherInstructor",
                    Role = Role.Instructor
                }
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateVideoAsync(1, "video.mp4", "original.mp4", 2, "Instructor"));

            Assert.Equal("You can only upload videos for your own courses.", ex.Message);
        }

        [Fact]
        public async Task ClearVideoAsync_Should_Clear_Video_Fields()
        {
            var course = new Course
            {
                Id = 1,
                VideoFileName = "video.mp4",
                VideoOriginalName = "original.mp4"
            };

            _courseRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(course);

            _courseRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            await _service.ClearVideoAsync(1);

            Assert.Null(course.VideoFileName);
            Assert.Null(course.VideoOriginalName);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(course), Times.Once);
        }
    }
}