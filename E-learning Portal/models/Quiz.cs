namespace E_learning_Portal.models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string QuestionsJson { get; set; } = null!; // store questions in JSON for simplicity
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public int InstructorId { get; set; }
        public User Instructor { get; set; } = null!;
    }
}
