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
    public class UserRepositoryTests
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
            context.Users.AddRange(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Role = Role.Admin,
                    PasswordHash = "x"
                },
                new User
                {
                    Id = 2,
                    Username = "student",
                    Role = Role.Student,
                    PasswordHash = "x"
                },
                new User
                {
                    Id = 3,
                    Username = "instructor",
                    Role = Role.Instructor,
                    PasswordHash = "x"
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetByUsernameAsync_Should_Return_User()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new UserRepository(context);

            var result = await repo.GetByUsernameAsync("student");

            Assert.NotNull(result);
            Assert.Equal("student", result!.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new UserRepository(context);

            var result = await repo.GetByUsernameAsync("unknown");

            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new UserRepository(context);

            var result = await repo.ExistsAsync("admin");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new UserRepository(context);

            var result = await repo.ExistsAsync("notexist");

            Assert.False(result);
        }

        [Fact]
        public async Task GetByRoleAsync_Should_Return_Users_By_Role()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new UserRepository(context);

            var result = await repo.GetByRoleAsync(Role.Student);

            Assert.Single(result);
            Assert.Equal(Role.Student, result.First().Role);
        }

        [Fact]
        public async Task GetByRoleAsync_Should_Return_Empty()
        {
            var context = GetDbContext();

            var repo = new UserRepository(context);

            var result = await repo.GetByRoleAsync(Role.Admin);

            Assert.Empty(result);
        }
    }
}