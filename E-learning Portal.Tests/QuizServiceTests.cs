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
    public class QuizServiceTests
    {
        private readonly Mock<IQuizRepository> _quizRepo;
        private readonly Mock<ICourseRepository> _courseRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly QuizService _service;

        public QuizServiceTests()
        {
            _quizRepo = new Mock<IQuizRepository>();
            _courseRepo = new Mock<ICourseRepository>();
            _userRepo = new Mock<IUserRepository>();

            _service = new QuizService(
                _quizRepo.Object,
                _courseRepo.Object,
                _userRepo.Object
            );
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Quiz()
        {
            var dto = new QuizCreateDTO
            {
                Title = "Quiz 1",
                CourseId = 1,
                QuestionsJson = "[]"
            };

            var course = new Course
            {
                Id = 1,
                InstructorId = 2,
                Instructor = new User { Username = "mani" }
            };

            var instructor = new User
            {
                Id = 2,
                Username = "mani",
                Role = Role.Instructor
            };

            _courseRepo.Setup(x => x.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            _userRepo.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(instructor);

            _quizRepo.Setup(x => x.AddAsync(It.IsAny<Quiz>()))
                .ReturnsAsync((Quiz q) => q);

            var result = await _service.CreateAsync(dto, 2, "Instructor");

            Assert.Equal("Quiz 1", result.Title);
            Assert.Equal(1, result.CourseId);
            Assert.Equal("mani", result.InstructorName);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Not_Owner_Instructor()
        {
            _courseRepo.Setup(x => x.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(new Course { InstructorId = 99 });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.CreateAsync(new QuizCreateDTO
                {
                    CourseId = 1,
                    Title = "Test",
                    QuestionsJson = "[]"
                }, 2, "Instructor"));
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Course_Not_Found()
        {
            _courseRepo.Setup(x => x.GetByIdWithInstructorAsync(1))
                .ReturnsAsync((Course?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateAsync(new QuizCreateDTO
                {
                    CourseId = 1
                }, 2, "Instructor"));

            Assert.Equal("Course not found.", ex.Message);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_List()
        {
            var list = new List<Quiz>
            {
                new Quiz
                {
                    Id = 1,
                    Title = "Quiz",
                    Course = new Course { Title = "Java" },
                    Instructor = new User { Username = "mani" }
                }
            };

            _quizRepo.Setup(x => x.GetByCourseAsync(1)).ReturnsAsync(list);

            var result = await _service.GetByCourseAsync(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Quiz()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Title = "Quiz",
                Course = new Course { Title = "Java" },
                Instructor = new User { Username = "mani" }
            };

            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(quiz);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal("Quiz", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_Not_Found()
        {
            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Quiz?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_When_Admin()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Course = new Course { InstructorId = 2 }
            };

            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(quiz);

            _quizRepo.Setup(x => x.DeleteAsync(quiz))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(1, 99, "Admin");

            _quizRepo.Verify(x => x.DeleteAsync(quiz), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Not_Owner()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Course = new Course { InstructorId = 99 }
            };

            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(quiz);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteAsync(1, 2, "Instructor"));
        }
    }
}