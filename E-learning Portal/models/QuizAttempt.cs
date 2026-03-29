namespace E_learning_Portal.models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;
        public int Score { get; set; }          // number of correct answers
        public int TotalQuestions { get; set; } // total questions in quiz
        public string AnswersJson { get; set; } = null!; // student's answers stored as JSON
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    }
}