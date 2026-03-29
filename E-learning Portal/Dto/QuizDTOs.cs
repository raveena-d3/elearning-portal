namespace E_learning_Portal.Dto
{
    public class QuizCreateDTO
    {
        public string Title { get; set; } = null!;
        public string QuestionsJson { get; set; } = null!;
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
    }

    // E_learning_Portal/Dto/QuizResponseDTO.cs — ADD QuestionsJson field
    public class QuizResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = null!;
        public string QuestionsJson { get; set; } = null!;  // ← ADD THIS
    }
}
