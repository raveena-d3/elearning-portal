using E_learning_Portal.Dto;
using E_learning_Portal.models;

namespace E_learning_Portal.Helpers
{
    public static class Mapper
    {
        public static UserResponseDTO ToDTO(this User user) => new UserResponseDTO
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };

        // Course
        public static CourseResponseDTO ToDTO(this Course course) => new CourseResponseDTO
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor.Username,
            YoutubeLinks = string.IsNullOrEmpty(course.YoutubeLinks)
                     ? new List<string>()
                     : System.Text.Json.JsonSerializer.Deserialize<List<string>>(course.YoutubeLinks),

            VideoFileName = course.VideoFileName,
            VideoOriginalName = course.VideoOriginalName
        };

        // Enrollment
        public static EnrollmentResponseDTO ToDTO(this Enrollment enrollment) => new EnrollmentResponseDTO
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course.Title,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student.Username,
            EnrolledAt = enrollment.EnrolledAt
        };

        // Quiz
        public static QuizResponseDTO ToDTO(this Quiz quiz) => new QuizResponseDTO
        {
            Id = quiz.Id,
            Title = quiz.Title,
            CourseId = quiz.CourseId,
            CourseTitle = quiz.Course?.Title ?? "",
            InstructorId = quiz.InstructorId,
            InstructorName = quiz.Instructor?.Username ?? "",
            QuestionsJson = quiz.QuestionsJson   // ← ADD THIS
        };

        // Discussion
        public static DiscussionResponseDTO ToDTO(this Discussion discussion) => new DiscussionResponseDTO
        {
            Id = discussion.Id,
            CourseId = discussion.CourseId,
            CourseTitle = discussion.Course.Title,
            StudentId = discussion.StudentId,
            StudentName = discussion.Student.Username,
            Question = discussion.Question,
            Answer = discussion.Answer,
            AskedAt = discussion.AskedAt,
            AnsweredAt = discussion.AnsweredAt
        };
    }
}
