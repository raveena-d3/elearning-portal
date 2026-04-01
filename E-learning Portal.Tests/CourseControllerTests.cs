using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElearningAPI.Controllers;
using ElearningAPI.Data;
using ElearningAPI.Services;
using E_learning_Portal.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class CourseControllerTests
    {
        private readonly Mock<ICourseService> _courseServiceMock;
        private readonly ElearningDbContext _dbContext;
        private readonly CourseController _controller;

        public CourseControllerTests()
        {
            _courseServiceMock = new Mock<ICourseService>();

            var options = new DbContextOptionsBuilder<ElearningDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ElearningDbContext(options);

            _controller = new CourseController(_courseServiceMock.Object, _dbContext);
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Courses()
        {
            // Arrange
            var courses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO
                {
                    Id = 1,
                    Title = "Java",
                    Description = "Java course",
                    InstructorId = 2,
                    InstructorName = "mani"
                },
                new CourseResponseDTO
                {
                    Id = 2,
                    Title = "ASP.NET",
                    Description = "Dotnet course",
                    InstructorId = 3,
                    InstructorName = "trainer"
                }
            };

            _courseServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(courses);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CourseResponseDTO>>(okResult.Value);
            Assert.Equal(2, ((List<CourseResponseDTO>)returnValue).Count);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Course_Exists()
        {
            // Arrange
            var course = new CourseResponseDTO
            {
                Id = 1,
                Title = "Java",
                Description = "Java Basics",
                InstructorId = 2,
                InstructorName = "mani"
            };

            _courseServiceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CourseResponseDTO>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
            Assert.Equal("Java", returnValue.Title);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Service_Throws_Exception()
        {
            // Arrange
            _courseServiceMock
                .Setup(s => s.GetByIdAsync(999))
                .ThrowsAsync(new Exception("Course not found"));

            // Act
            var result = await _controller.GetById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction_When_Successful()
        {
            // Arrange
            var dto = new CourseCreateDTO
            {
                Title = "New Course",
                Description = "New Description"
            };

            var createdCourse = new CourseResponseDTO
            {
                Id = 10,
                Title = "New Course",
                Description = "New Description",
                InstructorId = 2,
                InstructorName = "admin"
            };

            _courseServiceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(createdCourse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);

            var returnValue = Assert.IsType<CourseResponseDTO>(createdAtActionResult.Value);
            Assert.Equal(10, returnValue.Id);
            Assert.Equal("New Course", returnValue.Title);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_Service_Throws_Exception()
        {
            // Arrange
            var dto = new CourseCreateDTO
            {
                Title = "Bad Course",
                Description = "Bad Description"
            };

            _courseServiceMock
                .Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new Exception("Creation failed"));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task GetVideo_Should_Return_NotFound_When_No_VideoFileName()
        {
            // Arrange
            var course = new CourseResponseDTO
            {
                Id = 1,
                Title = "Java",
                VideoFileName = null
            };

            _courseServiceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.GetVideo(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetVideo_Should_Return_NotFound_When_File_Does_Not_Exist()
        {
            // Arrange
            var course = new CourseResponseDTO
            {
                Id = 1,
                Title = "Java",
                VideoFileName = "missing.mp4"
            };

            _courseServiceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(course);

            // Act
            var result = await _controller.GetVideo(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }
    }
}