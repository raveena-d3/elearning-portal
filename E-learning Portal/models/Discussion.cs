namespace E_learning_Portal.models
{
    public class Discussion
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;
        public string Question { get; set; } = null!;
        public string? Answer { get; set; } // Answer from instructor
        public DateTime AskedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AnsweredAt { get; set; }
    }
}
