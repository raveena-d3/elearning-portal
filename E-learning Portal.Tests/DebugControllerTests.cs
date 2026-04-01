using System.Collections.Generic;
using System.Security.Claims;
using ElearningAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class DebugControllerTests
    {
        private readonly DebugController _controller;

        public DebugControllerTests()
        {
            _controller = new DebugController();
        }

        private void SetUser(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
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

        [Fact]
        public void GetClaims_Should_Return_User_Claims()
        {
            // Arrange
            SetUser("admin1", "Admin");

            // Act
            var result = _controller.GetClaims();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void AdminTest_Should_Return_Message()
        {
            // Act
            var result = _controller.AdminTest();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Admin role works!", okResult.Value);
        }

        [Fact]
        public void InstructorTest_Should_Return_Message()
        {
            var result = _controller.InstructorTest();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Instructor role works!", okResult.Value);
        }

        [Fact]
        public void StudentTest_Should_Return_Message()
        {
            var result = _controller.StudentTest();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Student role works!", okResult.Value);
        }
    }
}