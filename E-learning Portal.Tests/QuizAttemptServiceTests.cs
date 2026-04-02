using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using ElearningAPI.Repositories;
using ElearningAPI.Services;
using E_learning_Portal.Dto;
using E_learning_Portal.models;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class QuizAttemptServiceTests
    {
        private readonly Mock<IQuizAttemptRepository> _attemptRepo;
        private readonly Mock<IQuizRepository> _quizRepo;
        private readonly Mock<IEnrollmentRepository> _enrollmentRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly QuizAttemptService _service;

        public QuizAttemptServiceTests()
        {
            _attemptRepo = new Mock<IQuizAttemptRepository>();
            _quizRepo = new Mock<IQuizRepository>();
            _enrollmentRepo = new Mock<IEnrollmentRepository>();
            _userRepo = new Mock<IUserRepository>();

            _service = new QuizAttemptService(
                _attemptRepo.Object,
                _quizRepo.Object,
                _enrollmentRepo.Object,
                _userRepo.Object
            );
        }

        [Fact]
        public async Task SubmitAttemptAsync_Should_Calculate_Score()
        {
            var questions = new List<QuizQuestion>
            {
                new QuizQuestion { Question = "Q1", CorrectAnswer = "A", Options = new List<string>() },
                new QuizQuestion { Question = "Q2", CorrectAnswer = "B", Options = new List<string>() }
            };

            var quiz = new Quiz
            {
                Id = 1,
                CourseId = 1,
                Title = "Test Quiz",
                QuestionsJson = JsonSerializer.Serialize(questions)
            };

            var student = new User
            {
                Id = 10,
                Username = "student",
                Role = Role.Student
            };

            var dto = new QuizAttemptCreateDTO
            {
                QuizId = 1,
                StudentId = 10,
                Answers = new List<string> { "A", "B" }
            };

            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(quiz);

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            _attemptRepo.Setup(x => x.GetByStudentAndQuizAsync(10, 1))
                .ReturnsAsync((QuizAttempt?)null);

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(student);

            _attemptRepo.Setup(x => x.AddAsync(It.IsAny<QuizAttempt>()))
                .ReturnsAsync((QuizAttempt a) => a);

            var result = await _service.SubmitAttemptAsync(dto);

            Assert.Equal(2, result.Score);
            Assert.Equal(100, result.Percentage);
            Assert.Equal("Test Quiz", result.QuizTitle);
            Assert.Equal("student", result.StudentName);
        }

        [Fact]
        public async Task SubmitAttemptAsync_Should_Throw_When_Quiz_Not_Found()
        {
            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Quiz?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitAttemptAsync(new QuizAttemptCreateDTO
                {
                    QuizId = 1,
                    StudentId = 10,
                    Answers = new List<string>()
                }));

            Assert.Equal("Quiz not found.", ex.Message);
        }

        [Fact]
        public async Task SubmitAttemptAsync_Should_Throw_When_Not_Enrolled()
        {
            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Quiz
                {
                    Id = 1,
                    CourseId = 1,
                    QuestionsJson = "[]"
                });

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitAttemptAsync(new QuizAttemptCreateDTO
                {
                    QuizId = 1,
                    StudentId = 10,
                    Answers = new List<string>()
                }));

            Assert.Equal("You must be enrolled in this course to attempt the quiz.", ex.Message);
        }

        [Fact]
        public async Task SubmitAttemptAsync_Should_Throw_When_Already_Attempted()
        {
            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Quiz
                {
                    Id = 1,
                    CourseId = 1,
                    QuestionsJson = "[]"
                });

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            _attemptRepo.Setup(x => x.GetByStudentAndQuizAsync(10, 1))
                .ReturnsAsync(new QuizAttempt());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitAttemptAsync(new QuizAttemptCreateDTO
                {
                    QuizId = 1,
                    StudentId = 10,
                    Answers = new List<string>()
                }));

            Assert.Equal("You have already attempted this quiz.", ex.Message);
        }

        [Fact]
        public async Task SubmitAttemptAsync_Should_Throw_When_Student_Not_Found()
        {
            var questions = new List<QuizQuestion>
            {
                new QuizQuestion { Question = "Q1", CorrectAnswer = "A", Options = new List<string>() }
            };

            _quizRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Quiz
                {
                    Id = 1,
                    CourseId = 1,
                    Title = "Quiz",
                    QuestionsJson = JsonSerializer.Serialize(questions)
                });

            _enrollmentRepo.Setup(x => x.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            _attemptRepo.Setup(x => x.GetByStudentAndQuizAsync(10, 1))
                .ReturnsAsync((QuizAttempt?)null);

            _userRepo.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitAttemptAsync(new QuizAttemptCreateDTO
                {
                    QuizId = 1,
                    StudentId = 10,
                    Answers = new List<string> { "A" }
                }));

            Assert.Equal("Student not found.", ex.Message);
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_List()
        {
            var attempts = new List<QuizAttempt>
            {
                new QuizAttempt
                {
                    Id = 1,
                    QuizId = 1,
                    StudentId = 10,
                    Quiz = new Quiz { Title = "Quiz 1" },
                    Student = new User { Username = "student" },
                    Score = 1,
                    TotalQuestions = 2,
                    AnswersJson = "[]",
                    AttemptedAt = DateTime.UtcNow
                }
            };

            _attemptRepo.Setup(x => x.GetByStudentAsync(10))
                .ReturnsAsync(attempts);

            var result = await _service.GetByStudentAsync(10);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByQuizAsync_Should_Return_List()
        {
            var attempts = new List<QuizAttempt>
            {
                new QuizAttempt
                {
                    Id = 1,
                    QuizId = 1,
                    StudentId = 10,
                    Quiz = new Quiz { Title = "Quiz 1" },
                    Student = new User { Username = "student" },
                    Score = 1,
                    TotalQuestions = 2,
                    AnswersJson = "[]",
                    AttemptedAt = DateTime.UtcNow
                }
            };

            _attemptRepo.Setup(x => x.GetByQuizAsync(1))
                .ReturnsAsync(attempts);

            var result = await _service.GetByQuizAsync(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Attempt()
        {
            var attempt = new QuizAttempt
            {
                Id = 1,
                QuizId = 1,
                StudentId = 10,
                Quiz = new Quiz { Title = "Quiz 1" },
                Student = new User { Username = "student" },
                Score = 1,
                TotalQuestions = 2,
                AnswersJson = "[]",
                AttemptedAt = DateTime.UtcNow
            };

            _attemptRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(attempt);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Quiz 1", result.QuizTitle);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_Not_Found()
        {
            _attemptRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((QuizAttempt?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(1));

            Assert.Equal("Attempt not found.", ex.Message);
        }
    }
}