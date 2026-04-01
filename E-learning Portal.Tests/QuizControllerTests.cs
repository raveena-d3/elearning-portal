using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using E_learning_Portal.Dto;
using E_learning_Portal.models;
using ElearningAPI.Controllers;
using ElearningAPI.Data;
using ElearningAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class QuizControllerTests
    {
        private readonly Mock<IQuizService> _quizServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly QuizController _controller;

        public QuizControllerTests()
        {
            _quizServiceMock = new Mock<IQuizService>();

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);
            _controller = new QuizController(_quizServiceMock.Object, _dbContext);
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
        public async Task Create_Should_Return_CreatedAtAction_When_Successful()
        {
            // Arrange
            await SeedUserAsync(2, "instructor1", Role.Instructor);
            SetUser("instructor1", "Instructor");

            var dto = new QuizCreateDTO
            {
                Title = "Java Quiz",
                CourseId = 10
            };

            var response = new QuizResponseDTO
            {
                Id = 1,
                Title = "Java Quiz",
                CourseId = 10,
                InstructorId = 2,
                InstructorName = "instructor1"
            };

            _quizServiceMock
                .Setup(s => s.CreateAsync(dto, 2, "Instructor"))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);

            var returnValue = Assert.IsType<QuizResponseDTO>(createdResult.Value);
            Assert.Equal(1, returnValue.Id);
            Assert.Equal("Java Quiz", returnValue.Title);
        }

        [Fact]
        public async Task Create_Should_Return_Forbid_When_UnauthorizedAccessException_Is_Thrown()
        {
            // Arrange
            await SeedUserAsync(2, "instructor1", Role.Instructor);
            SetUser("instructor1", "Instructor");

            var dto = new QuizCreateDTO
            {
                Title = "Java Quiz",
                CourseId = 10
            };

            _quizServiceMock
                .Setup(s => s.CreateAsync(dto, 2, "Instructor"))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetByCourse_Should_Return_Ok_With_Quizzes()
        {
            // Arrange
            var quizzes = new List<QuizResponseDTO>
            {
                new QuizResponseDTO
                {
                    Id = 1,
                    Title = "Java Quiz",
                    CourseId = 10
                }
            };

            _quizServiceMock
                .Setup(s => s.GetByCourseAsync(10))
                .ReturnsAsync(quizzes);

            // Act
            var result = await _controller.GetByCourse(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<QuizResponseDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Quiz_Exists()
        {
            // Arrange
            var quiz = new QuizResponseDTO
            {
                Id = 1,
                Title = "Java Quiz",
                CourseId = 10
            };

            _quizServiceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(quiz);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<QuizResponseDTO>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Service_Throws_Exception()
        {
            // Arrange
            _quizServiceMock
                .Setup(s => s.GetByIdAsync(999))
                .ThrowsAsync(new Exception("Quiz not found"));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_Successful()
        {
            // Arrange
            await SeedUserAsync(1, "admin1", Role.Admin);
            SetUser("admin1", "Admin");

            _quizServiceMock
                .Setup(s => s.DeleteAsync(1, 1, "Admin"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_Forbid_When_UnauthorizedAccessException_Is_Thrown()
        {
            // Arrange
            await SeedUserAsync(2, "instructor1", Role.Instructor);
            SetUser("instructor1", "Instructor");

            _quizServiceMock
                .Setup(s => s.DeleteAsync(1, 2, "Instructor"))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}