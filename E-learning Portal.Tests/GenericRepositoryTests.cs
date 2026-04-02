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
    public class GenericRepositoryTests
    {
        private ElearningDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElearningDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Entity()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            var user = new User
            {
                Username = "raveena",
                Role = Role.Student,
                PasswordHash = "test"
            };

            var result = await repo.AddAsync(user);

            Assert.NotNull(result);
            Assert.Equal(1, context.Users.Count());
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Entities()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            context.Users.AddRange(
                new User { Username = "user1", Role = Role.Student, PasswordHash = "x" },
                new User { Username = "user2", Role = Role.Student, PasswordHash = "x" }
            );
            await context.SaveChangesAsync();

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            var user = new User
            {
                Id = 1,
                Username = "test",
                Role = Role.Student,
                PasswordHash = "x"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("test", result!.Username);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Entity()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            var user = new User
            {
                Id = 1,
                Username = "old",
                Role = Role.Student,
                PasswordHash = "x"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.Username = "updated";

            await repo.UpdateAsync(user);

            var updated = await context.Users.FindAsync(1);

            Assert.Equal("updated", updated!.Username);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Entity()
        {
            var context = GetDbContext();
            var repo = new Repository<User>(context);

            var user = new User
            {
                Id = 1,
                Username = "delete",
                Role = Role.Student,
                PasswordHash = "x"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await repo.DeleteAsync(user);

            Assert.Empty(context.Users);
        }
    }
}