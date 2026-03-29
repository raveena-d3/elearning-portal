namespace E_learning_Portal.models
{
    public class Course
    {
        
            public int Id { get; set; }
            public string Title { get; set; } 
            public string Description { get; set; } 
            public int InstructorId { get; set; }
            public User Instructor { get; set; } = null!;
            public string? YoutubeLinks { get; set; }
            public string? VideoFileName { get; set; }
            public string? VideoOriginalName { get; set; }
     }
    }

