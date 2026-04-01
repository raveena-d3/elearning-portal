using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElearningAPI.Controllers;
using ElearningAPI.Data;
using ElearningAPI.Services;
using ElearningAPI.Helpers;
using E_learning_Portal.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();

            // ❌ REMOVE MOCKING → USE REAL INSTANCE
            var keycloak = new KeycloakAdminService(null!, null!);

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);

            _controller = new UserController(
                _userServiceMock.Object,
                keycloak,
                _dbContext
            );
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Users()
        {
            var users = new List<UserResponseDTO>
            {
                new UserResponseDTO { Id = 1, Username = "admin1", Role = "Admin" }
            };

            _userServiceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(users);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Single((IEnumerable<UserResponseDTO>)okResult.Value!);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok()
        {
            var user = new UserResponseDTO
            {
                Id = 1,
                Username = "admin1",
                Role = "Admin"
            };

            _userServiceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(user);

            var result = await _controller.GetById(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound()
        {
            _userServiceMock.Setup(s => s.GetByIdAsync(99))
                .ThrowsAsync(new Exception());

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_Should_Return_Ok()
        {
            var dto = new CreateUserDTO
            {
                Username = "student1",
                Password = "123",
                Role = "Student"
            };

            var response = new UserResponseDTO
            {
                Id = 1,
                Username = "student1",
                Role = "Student"
            };

            _userServiceMock.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(response);

            // ❌ NO MOCK HERE

            var result = await _controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRole_Should_Return_Ok()
        {
            var request = new UpdateRoleRequest { Role = "Admin" };

            _userServiceMock.Setup(s => s.UpdateRoleAsync(1, "Admin"))
                .ReturnsAsync(new UserResponseDTO { Id = 1, Role = "Admin" });

            var result = await _controller.UpdateRole(1, request);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent()
        {
            var user = new UserResponseDTO
            {
                Id = 1,
                Username = "user1",
                Role = "Student"
            };

            _userServiceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userServiceMock.Setup(s => s.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            // ❌ NO MOCK HERE

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_BadRequest()
        {
            _userServiceMock.Setup(s => s.GetByIdAsync(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Delete(1);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}