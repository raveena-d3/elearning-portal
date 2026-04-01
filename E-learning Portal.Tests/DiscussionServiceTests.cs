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
    public class DiscussionServiceTests
    {
        private readonly Mock<IDiscussionRepository> _discussionRepoMock;
        private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly DiscussionService _service;

        public DiscussionServiceTests()
        {
            _discussionRepoMock = new Mock<IDiscussionRepository>();
            _enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();

            _service = new DiscussionService(
                _discussionRepoMock.Object,
                _enrollmentRepoMock.Object,
                _courseRepoMock.Object
            );
        }

        [Fact]
        public async Task AskQuestionAsync_Should_Create_Question_When_Student_Enrolled()
        {
            var dto = new DiscussionCreateDTO
            {
                CourseId = 1,
                StudentId = 10,
                Question = "What is API?"
            };

            var course = new Course
            {
                Id = 1,
                Title = "Java",
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                }
            };

            var savedDiscussion = new Discussion
            {
                Id = 1,
                CourseId = 1,
                Course = course,
                StudentId = 10,
                Student = new User
                {
                    Id = 10,
                    Username = "student1",
                    Role = Role.Student
                },
                Question = dto.Question,
                AskedAt = DateTime.UtcNow
            };

            _courseRepoMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(course);

            _enrollmentRepoMock
                .Setup(r => r.IsEnrolledAsync(10, 1))
                .ReturnsAsync(true);

            _discussionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Discussion>()))
                .ReturnsAsync((Discussion d) =>
                {
                    d.Id = 1;
                    return d;
                });

            _discussionRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(savedDiscussion);

            var result = await _service.AskQuestionAsync(dto);

            Assert.Equal("What is API?", result.Question);
            Assert.Equal("student1", result.StudentName);
            Assert.Equal(1, result.CourseId);
        }

        [Fact]
        public async Task AskQuestionAsync_Should_Throw_When_Course_Not_Found()
        {
            var dto = new DiscussionCreateDTO
            {
                CourseId = 1,
                StudentId = 10,
                Question = "Test"
            };

            _courseRepoMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync((Course?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AskQuestionAsync(dto));

            Assert.Equal("Course not found.", ex.Message);
        }

        [Fact]
        public async Task AskQuestionAsync_Should_Throw_When_Not_Enrolled()
        {
            var dto = new DiscussionCreateDTO
            {
                CourseId = 1,
                StudentId = 10,
                Question = "Test"
            };

            _courseRepoMock
                .Setup(r => r.GetByIdWithInstructorAsync(1))
                .ReturnsAsync(new Course
                {
                    Id = 1,
                    Instructor = new User { Username = "mani" }
                });

            _enrollmentRepoMock
                .Setup(r => r.IsEnrolledAsync(10, 1))
                .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AskQuestionAsync(dto));

            Assert.Equal("Student must be enrolled in the course to ask questions.", ex.Message);
        }

        [Fact]
        public async Task AnswerQuestionAsync_Should_Update_When_Authorized()
        {
            var discussion = new Discussion
            {
                Id = 1,
                CourseId = 1,
                Course = new Course
                {
                    Id = 1,
                    Title = "Java",
                    InstructorId = 2
                },
                StudentId = 10,
                Student = new User
                {
                    Id = 10,
                    Username = "student1",
                    Role = Role.Student
                },
                Question = "What is API?"
            };

            _discussionRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(discussion);

            _discussionRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Discussion>()))
                .Returns(Task.CompletedTask);

            var result = await _service.AnswerQuestionAsync(1, "Answer", 2, "Instructor");

            Assert.Equal("Answer", result.Answer);
        }

        [Fact]
        public async Task AnswerQuestionAsync_Should_Throw_When_Discussion_Not_Found()
        {
            _discussionRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Discussion?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AnswerQuestionAsync(1, "Answer", 2, "Instructor"));

            Assert.Equal("Discussion not found.", ex.Message);
        }

        [Fact]
        public async Task AnswerQuestionAsync_Should_Throw_Unauthorized()
        {
            var discussion = new Discussion
            {
                Id = 1,
                Course = new Course
                {
                    InstructorId = 99
                },
                Student = new User
                {
                    Username = "student1"
                }
            };

            _discussionRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(discussion);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.AnswerQuestionAsync(1, "Answer", 2, "Instructor"));

            Assert.Equal("You can only answer questions in your own courses.", ex.Message);
        }

        [Fact]
        public async Task GetByCourseAsync_Should_Return_DTO_List()
        {
            var discussions = new List<Discussion>
            {
                new Discussion
                {
                    Id = 1,
                    CourseId = 1,
                    Course = new Course
                    {
                        Title = "Java"
                    },
                    StudentId = 10,
                    Student = new User
                    {
                        Username = "student1"
                    },
                    Question = "Q1"
                }
            };

            _discussionRepoMock
                .Setup(r => r.GetByCourseAsync(1))
                .ReturnsAsync(discussions);

            var result = await _service.GetByCourseAsync(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByStudentAsync_Should_Return_DTO_List()
        {
            var discussions = new List<Discussion>
            {
                new Discussion
                {
                    Id = 1,
                    StudentId = 10,
                    Student = new User
                    {
                        Username = "student1"
                    },
                    Course = new Course
                    {
                        Title = "Java"
                    },
                    Question = "Q1"
                }
            };

            _discussionRepoMock
                .Setup(r => r.GetByStudentAsync(10))
                .ReturnsAsync(discussions);

            var result = await _service.GetByStudentAsync(10);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByInstructorAsync_Should_Return_All_Course_Discussions()
        {
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    InstructorId = 2,
                    Instructor = new User { Username = "mani" }
                },
                new Course
                {
                    Id = 2,
                    InstructorId = 2,
                    Instructor = new User { Username = "mani" }
                }
            };

            var discussions = new List<Discussion>
            {
                new Discussion
                {
                    Id = 1,
                    CourseId = 1,
                    Course = new Course { Title = "Java" },
                    Student = new User { Username = "student1" },
                    Question = "Q1",
                    AskedAt = DateTime.UtcNow
                }
            };

            _courseRepoMock
                .Setup(r => r.GetByInstructorAsync(2))
                .ReturnsAsync(courses);

            _discussionRepoMock
                .Setup(r => r.GetByCourseAsync(It.IsAny<int>()))
                .ReturnsAsync(discussions);

            var result = await _service.GetByInstructorAsync(2);

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_When_Admin()
        {
            var discussion = new Discussion
            {
                Id = 1,
                StudentId = 10
            };

            _discussionRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(discussion);

            _discussionRepoMock
                .Setup(r => r.DeleteAsync(discussion))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(1, 99, "Admin");

            _discussionRepoMock.Verify(r => r.DeleteAsync(discussion), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Discussion_Not_Found()
        {
            _discussionRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Discussion?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.DeleteAsync(1, 99, "Admin"));

            Assert.Equal("Discussion not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Student_Deletes_Other()
        {
            var discussion = new Discussion
            {
                Id = 1,
                StudentId = 20
            };

            _discussionRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(discussion);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteAsync(1, 10, "Student"));

            Assert.Equal("You can only delete your own questions.", ex.Message);
        }
    }
}