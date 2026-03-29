namespace E_learning_Portal.Dto
{
    // What student sends when submitting quiz
    public class QuizAttemptCreateDTO
    {
        public int QuizId { get; set; }
        public int StudentId { get; set; }
        public List<string> Answers { get; set; } = new(); // answers in order matching questions
    }

    // What we return after grading
    public class QuizAttemptResponseDTO
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public string AnswersJson { get; set; } = null!;
        public DateTime AttemptedAt { get; set; }
    }
}