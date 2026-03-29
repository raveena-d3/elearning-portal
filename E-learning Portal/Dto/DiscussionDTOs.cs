namespace E_learning_Portal.Dto
{
    public class DiscussionCreateDTO
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public string Question { get; set; } = null!;
    }

    public class DiscussionResponseDTO
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public DateTime AskedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
    }
}
