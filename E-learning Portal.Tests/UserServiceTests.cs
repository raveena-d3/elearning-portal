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
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepo = new Mock<IUserRepository>();
            _service = new UserService(_userRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_List()
        {
            var users = new List<User>
            {
                new User { Id = 1, Username = "mani", Role = Role.Admin }
            };

            _userRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

            var result = await _service.GetAllAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_User()
        {
            var user = new User
            {
                Id = 1,
                Username = "mani",
                Role = Role.Admin
            };

            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal("mani", result.Username);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_Not_Found()
        {
            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(1));
        }

        [Fact]
        public async Task CreateAsync_Should_Create_User()
        {
            var dto = new CreateUserDTO
            {
                Username = "student",
                Password = "123",
                Role = "Student"
            };

            _userRepo.Setup(x => x.GetByUsernameAsync("student"))
                .ReturnsAsync((User?)null);

            _userRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var result = await _service.CreateAsync(dto);

            Assert.Equal("student", result.Username);
            Assert.Equal("Student", result.Role);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Username_Exists()
        {
            _userRepo.Setup(x => x.GetByUsernameAsync("student"))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateAsync(new CreateUserDTO
                {
                    Username = "student",
                    Role = "Student"
                }));
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Invalid_Role()
        {
            _userRepo.Setup(x => x.GetByUsernameAsync("test"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateAsync(new CreateUserDTO
                {
                    Username = "test",
                    Role = "InvalidRole"
                }));
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_Update()
        {
            var user = new User { Id = 1, Role = Role.Student };

            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepo.Setup(x => x.UpdateAsync(user))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateRoleAsync(1, "Admin");

            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task UpdateRoleAsync_Should_Throw_When_Invalid_Role()
        {
            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateRoleAsync(1, "Invalid"));
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_User()
        {
            var user = new User { Id = 1 };

            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepo.Setup(x => x.DeleteAsync(user))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(1);

            _userRepo.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Not_Found()
        {
            _userRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.DeleteAsync(1));
        }
    }
}