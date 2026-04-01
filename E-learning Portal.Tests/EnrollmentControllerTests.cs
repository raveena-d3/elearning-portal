using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ElearningAPI.Controllers;
using ElearningAPI.Data;
using ElearningAPI.Services;
using E_learning_Portal.Dto;
using E_learning_Portal.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class EnrollmentControllerTests
    {
        private readonly Mock<IEnrollmentService> _enrollmentServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly EnrollmentController _controller;

        public EnrollmentControllerTests()
        {
            _enrollmentServiceMock = new Mock<IEnrollmentService>();

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);
            _controller = new EnrollmentController(_enrollmentServiceMock.Object, _dbContext);
        }

        private void SetUser(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim("preferred_username", username),
                new Claim(ClaimTypes.Role, role),
                new Claim("roles", role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        private async Task SeedUserAsync(int id, string username, Role role)
        {
            _dbContext.Users.Add(new User
            {
                Id = id,
                Username = username,
                Role = role
            });

            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task Enroll_Should_Return_Ok_And_Set_StudentId_From_LoggedIn_User()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            var dto = new EnrollmentCreateDTO
            {
                CourseId = 10
            };

            var response = new EnrollmentResponseDTO
            {
                Id = 1,
                CourseId = 10,
                StudentId = 5,
                StudentName = "student1",
                CourseTitle = "Java",
                EnrolledAt = new DateTime(2026, 4, 1)
            };

            _enrollmentServiceMock
                .Setup(s => s.EnrollAsync(It.IsAny<EnrollmentCreateDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Enroll(dto);

            // Assert
            Assert.Equal(5, dto.StudentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EnrollmentResponseDTO>(okResult.Value);
            Assert.Equal(5, returnValue.StudentId);
            Assert.Equal(10, returnValue.CourseId);
        }

        [Fact]
        public async Task GetByStudent_Should_Return_Forbid_When_Student_Tries_To_Access_Other_Student_Data()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            // Act
            var result = await _controller.GetByStudent(99);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetByCourse_Should_Return_Ok_With_Enrollments()
        {
            // Arrange
            var enrollments = new List<EnrollmentResponseDTO>
            {
                new EnrollmentResponseDTO
                {
                    Id = 1,
                    CourseId = 10,
                    CourseTitle = "Java",
                    StudentId = 5,
                    StudentName = "student1",
                    EnrolledAt = new DateTime(2026, 4, 1)
                }
            };

            _enrollmentServiceMock
                .Setup(s => s.GetByCourseAsync(10))
                .ReturnsAsync(enrollments);

            // Act
            var result = await _controller.GetByCourse(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<EnrollmentResponseDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task IsEnrolled_Should_Return_Ok_With_True_Value()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            _enrollmentServiceMock
                .Setup(s => s.IsEnrolledAsync(5, 10))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IsEnrolled(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var valueType = okResult.Value!.GetType();
            var property = valueType.GetProperty("isEnrolled");
            Assert.NotNull(property);

            var propertyValue = property!.GetValue(okResult.Value);
            Assert.Equal(true, propertyValue);
        }

        [Fact]
        public async Task GetMyEnrollments_Should_Return_Unauthorized_When_UserId_Is_Zero()
        {
            // Arrange
            SetUser("unknown_user", "Student");

            // Act
            var result = await _controller.GetMyEnrollments();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Unenroll_Should_Return_NoContent_When_Successful()
        {
            // Arrange
            await SeedUserAsync(1, "admin1", Role.Admin);
            SetUser("admin1", "Admin");

            _enrollmentServiceMock
                .Setup(s => s.UnenrollAsync(1, 1, "Admin"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Unenroll(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Unenroll_Should_Return_Forbid_When_UnauthorizedAccessException_Is_Thrown()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            _enrollmentServiceMock
                .Setup(s => s.UnenrollAsync(1, 5, "Student"))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Unenroll(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}