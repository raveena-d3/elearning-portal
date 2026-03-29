namespace E_learning_Portal.Dto
{
    public class EnrollmentCreateDTO
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
    }

    public class EnrollmentResponseDTO
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
    }
}
