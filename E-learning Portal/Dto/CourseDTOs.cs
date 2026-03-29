namespace E_learning_Portal.Dto
{
    public class CourseCreateDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int InstructorId { get; set; }
        public List<string>? YoutubeLinks { get; set; }
    }

    public class CourseResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = null!;
        public List<string>? YoutubeLinks { get; set; }
        public string? VideoFileName { get; set; }
        public string? VideoOriginalName { get; set; }
    }
}
