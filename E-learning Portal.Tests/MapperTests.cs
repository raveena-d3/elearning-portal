using System;
using System.Collections.Generic;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;
using Xunit;

namespace E_learning_Portal.Tests
{
    public class MapperTests
    {
        [Fact]
        public void User_ToDTO_Should_Map_Correctly()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "raveena",
                Role = Role.Student
            };

            // Act
            UserResponseDTO result = user.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("raveena", result.Username);
            Assert.Equal("Student", result.Role);
        }

        [Fact]
        public void Course_ToDTO_Should_Map_Correctly_With_YoutubeLinks()
        {
            // Arrange
            var course = new Course
            {
                Id = 10,
                Title = "Java Basics",
                Description = "Intro to Java",
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                },
                YoutubeLinks = "[\"https://youtu.be/a1\",\"https://youtu.be/a2\"]",
                VideoFileName = "java.mp4",
                VideoOriginalName = "java_original.mp4"
            };

            // Act
            CourseResponseDTO result = course.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Java Basics", result.Title);
            Assert.Equal("Intro to Java", result.Description);
            Assert.Equal(2, result.InstructorId);
            Assert.Equal("mani", result.InstructorName);
            Assert.NotNull(result.YoutubeLinks);
            Assert.Equal(2, result.YoutubeLinks.Count);
            Assert.Equal("https://youtu.be/a1", result.YoutubeLinks[0]);
            Assert.Equal("https://youtu.be/a2", result.YoutubeLinks[1]);
            Assert.Equal("java.mp4", result.VideoFileName);
            Assert.Equal("java_original.mp4", result.VideoOriginalName);
        }

        [Fact]
        public void Course_ToDTO_Should_Return_Empty_YoutubeLinks_When_Null()
        {
            // Arrange
            var course = new Course
            {
                Id = 11,
                Title = "Spring Boot",
                Description = "Backend course",
                InstructorId = 3,
                Instructor = new User
                {
                    Id = 3,
                    Username = "instructor1",
                    Role = Role.Instructor
                },
                YoutubeLinks = null,
                VideoFileName = "spring.mp4",
                VideoOriginalName = "spring_original.mp4"
            };

            // Act
            CourseResponseDTO result = course.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.YoutubeLinks);
            Assert.Empty(result.YoutubeLinks);
        }

        [Fact]
        public void Course_ToDTO_Should_Return_Empty_YoutubeLinks_When_Empty()
        {
            // Arrange
            var course = new Course
            {
                Id = 12,
                Title = "ASP.NET Core",
                Description = "API development",
                InstructorId = 4,
                Instructor = new User
                {
                    Id = 4,
                    Username = "trainer",
                    Role = Role.Instructor
                },
                YoutubeLinks = "",
                VideoFileName = "dotnet.mp4",
                VideoOriginalName = "dotnet_original.mp4"
            };

            // Act
            CourseResponseDTO result = course.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.YoutubeLinks);
            Assert.Empty(result.YoutubeLinks);
        }

        [Fact]
        public void Enrollment_ToDTO_Should_Map_Correctly()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                Id = 100,
                CourseId = 10,
                Course = new Course
                {
                    Id = 10,
                    Title = "Java Basics"
                },
                StudentId = 5,
                Student = new User
                {
                    Id = 5,
                    Username = "student1",
                    Role = Role.Student
                },
                EnrolledAt = new DateTime(2026, 3, 1)
            };

            // Act
            EnrollmentResponseDTO result = enrollment.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal(10, result.CourseId);
            Assert.Equal("Java Basics", result.CourseTitle);
            Assert.Equal(5, result.StudentId);
            Assert.Equal("student1", result.StudentName);
            Assert.Equal(new DateTime(2026, 3, 1), result.EnrolledAt);
        }

        [Fact]
        public void Quiz_ToDTO_Should_Map_Correctly_When_Navigation_Properties_Exist()
        {
            // Arrange
            var quiz = new Quiz
            {
                Id = 200,
                Title = "Java Quiz",
                CourseId = 10,
                Course = new Course
                {
                    Id = 10,
                    Title = "Java Basics"
                },
                InstructorId = 2,
                Instructor = new User
                {
                    Id = 2,
                    Username = "mani",
                    Role = Role.Instructor
                },
                QuestionsJson = "[{\"question\":\"What is Java?\"}]"
            };

            // Act
            QuizResponseDTO result = quiz.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
            Assert.Equal("Java Quiz", result.Title);
            Assert.Equal(10, result.CourseId);
            Assert.Equal("Java Basics", result.CourseTitle);
            Assert.Equal(2, result.InstructorId);
            Assert.Equal("mani", result.InstructorName);
            Assert.Equal("[{\"question\":\"What is Java?\"}]", result.QuestionsJson);
        }

        [Fact]
        public void Quiz_ToDTO_Should_Map_Empty_Strings_When_Navigation_Properties_Are_Null()
        {
            // Arrange
            var quiz = new Quiz
            {
                Id = 201,
                Title = "Null Quiz",
                CourseId = 20,
                Course = null,
                InstructorId = 8,
                Instructor = null,
                QuestionsJson = "[]"
            };

            // Act
            QuizResponseDTO result = quiz.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.CourseTitle);
            Assert.Equal("", result.InstructorName);
            Assert.Equal("[]", result.QuestionsJson);
        }

        [Fact]
        public void Discussion_ToDTO_Should_Map_Correctly()
        {
            // Arrange
            var discussion = new Discussion
            {
                Id = 300,
                CourseId = 10,
                Course = new Course
                {
                    Id = 10,
                    Title = "Java Basics"
                },
                StudentId = 5,
                Student = new User
                {
                    Id = 5,
                    Username = "student1",
                    Role = Role.Student
                },
                Question = "What is JVM?",
                Answer = "Java Virtual Machine",
                AskedAt = new DateTime(2026, 3, 2, 10, 0, 0),
                AnsweredAt = new DateTime(2026, 3, 2, 11, 0, 0)
            };

            // Act
            DiscussionResponseDTO result = discussion.ToDTO();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.Id);
            Assert.Equal(10, result.CourseId);
            Assert.Equal("Java Basics", result.CourseTitle);
            Assert.Equal(5, result.StudentId);
            Assert.Equal("student1", result.StudentName);
            Assert.Equal("What is JVM?", result.Question);
            Assert.Equal("Java Virtual Machine", result.Answer);
            Assert.Equal(new DateTime(2026, 3, 2, 10, 0, 0), result.AskedAt);
            Assert.Equal(new DateTime(2026, 3, 2, 11, 0, 0), result.AnsweredAt);
        }
    }
}