using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using E_learning_Portal.Dto;
using E_learning_Portal.Service.Interface;
using E_learning_Portal.models;
using ElearningAPI.Controllers;
using ElearningAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class QuizAttemptControllerTests
    {
        private readonly Mock<IQuizAttemptService> _attemptServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly QuizAttemptController _controller;

        public QuizAttemptControllerTests()
        {
            _attemptServiceMock = new Mock<IQuizAttemptService>();

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);
            _controller = new QuizAttemptController(_attemptServiceMock.Object, _dbContext);
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
        public async Task Submit_Should_Return_Ok_And_Set_StudentId_From_LoggedIn_User()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            var dto = new QuizAttemptCreateDTO
            {
                QuizId = 10
            };

            var response = new QuizAttemptResponseDTO
            {
                Id = 1,
                QuizId = 10,
                StudentId = 5
            };

            _attemptServiceMock
                .Setup(s => s.SubmitAttemptAsync(It.IsAny<QuizAttemptCreateDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Submit(dto);

            // Assert
            Assert.Equal(5, dto.StudentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<QuizAttemptResponseDTO>(okResult.Value);
            Assert.Equal(5, returnValue.StudentId);
            Assert.Equal(10, returnValue.QuizId);
        }

        [Fact]
        public async Task Submit_Should_Return_BadRequest_When_Service_Throws_Exception()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            var dto = new QuizAttemptCreateDTO
            {
                QuizId = 10
            };

            _attemptServiceMock
                .Setup(s => s.SubmitAttemptAsync(It.IsAny<QuizAttemptCreateDTO>()))
                .ThrowsAsync(new Exception("Submit failed"));

            // Act
            var result = await _controller.Submit(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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
        public async Task GetMyAttempts_Should_Return_Unauthorized_When_UserId_Is_Zero()
        {
            // Arrange
            SetUser("unknown_user", "Student");

            // Act
            var result = await _controller.GetMyAttempts();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetByQuiz_Should_Return_Ok_With_Attempts()
        {
            // Arrange
            var attempts = new List<QuizAttemptResponseDTO>
            {
                new QuizAttemptResponseDTO
                {
                    Id = 1,
                    QuizId = 10,
                    StudentId = 5
                }
            };

            _attemptServiceMock
                .Setup(s => s.GetByQuizAsync(10))
                .ReturnsAsync(attempts);

            // Act
            var result = await _controller.GetByQuiz(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<QuizAttemptResponseDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Attempt_Exists()
        {
            // Arrange
            var attempt = new QuizAttemptResponseDTO
            {
                Id = 1,
                QuizId = 10,
                StudentId = 5
            };

            _attemptServiceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(attempt);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<QuizAttemptResponseDTO>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Service_Throws_Exception()
        {
            // Arrange
            _attemptServiceMock
                .Setup(s => s.GetByIdAsync(999))
                .ThrowsAsync(new Exception("Attempt not found"));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}