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
    public class DiscussionControllerTests
    {
        private readonly Mock<IDiscussionService> _discussionServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly DiscussionController _controller;

        public DiscussionControllerTests()
        {
            _discussionServiceMock = new Mock<IDiscussionService>();

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);
            _controller = new DiscussionController(_discussionServiceMock.Object, _dbContext);
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
        public async Task GetByCourse_Should_Return_Ok_With_Discussions()
        {
            // Arrange
            var discussions = new List<DiscussionResponseDTO>
            {
                new DiscussionResponseDTO
                {
                    Id = 1,
                    CourseId = 10,
                    CourseTitle = "Java",
                    StudentId = 5,
                    StudentName = "student1",
                    Question = "What is JVM?"
                }
            };

            _discussionServiceMock
                .Setup(s => s.GetByCourseAsync(10))
                .ReturnsAsync(discussions);

            // Act
            var result = await _controller.GetByCourse(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<DiscussionResponseDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task AskQuestion_Should_Return_Ok_And_Set_StudentId_From_LoggedIn_User()
        {
            // Arrange
            await SeedUserAsync(5, "student1", Role.Student);
            SetUser("student1", "Student");

            var dto = new DiscussionCreateDTO
            {
                CourseId = 10,
                Question = "What is JVM?"
            };

            var response = new DiscussionResponseDTO
            {
                Id = 1,
                CourseId = 10,
                StudentId = 5,
                StudentName = "student1",
                Question = "What is JVM?"
            };

            _discussionServiceMock
                .Setup(s => s.AskQuestionAsync(It.IsAny<DiscussionCreateDTO>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AskQuestion(dto);

            // Assert
            Assert.Equal(5, dto.StudentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<DiscussionResponseDTO>(okResult.Value);
            Assert.Equal(5, returnValue.StudentId);
            Assert.Equal("What is JVM?", returnValue.Question);
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
        public async Task GetMyDiscussions_Should_Return_Unauthorized_When_UserId_Is_Zero()
        {
            // Arrange
            SetUser("unknown_user", "Student");

            // Act
            var result = await _controller.GetMyDiscussions();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task AnswerQuestion_Should_Return_Ok_When_Successful()
        {
            // Arrange
            await SeedUserAsync(2, "instructor1", Role.Instructor);
            SetUser("instructor1", "Instructor");

            var dto = new AnswerDTO
            {
                Answer = "JVM means Java Virtual Machine"
            };

            var response = new DiscussionResponseDTO
            {
                Id = 1,
                CourseId = 10,
                StudentId = 5,
                StudentName = "student1",
                Question = "What is JVM?",
                Answer = "JVM means Java Virtual Machine"
            };

            _discussionServiceMock
                .Setup(s => s.AnswerQuestionAsync(1, dto.Answer, 2, "Instructor"))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AnswerQuestion(1, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<DiscussionResponseDTO>(okResult.Value);
            Assert.Equal("JVM means Java Virtual Machine", returnValue.Answer);
        }

        [Fact]
        public async Task AnswerQuestion_Should_Return_Forbid_When_UnauthorizedAccessException_Is_Thrown()
        {
            // Arrange
            await SeedUserAsync(2, "instructor1", Role.Instructor);
            SetUser("instructor1", "Instructor");

            var dto = new AnswerDTO
            {
                Answer = "Answer text"
            };

            _discussionServiceMock
                .Setup(s => s.AnswerQuestionAsync(1, dto.Answer, 2, "Instructor"))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.AnswerQuestion(1, dto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_Successful()
        {
            // Arrange
            await SeedUserAsync(1, "admin1", Role.Admin);
            SetUser("admin1", "Admin");

            _discussionServiceMock
                .Setup(s => s.DeleteAsync(1, 1, "Admin"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}