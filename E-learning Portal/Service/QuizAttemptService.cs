using E_learning_Portal.Dto;
using E_learning_Portal.models;
using E_learning_Portal.Service.Interface;
using ElearningAPI.Repositories;
using System.Text.Json;

namespace ElearningAPI.Services
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizAttemptRepository _attemptRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;

        public QuizAttemptService(
            IQuizAttemptRepository attemptRepository,
            IQuizRepository quizRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository)
        {
            _attemptRepository = attemptRepository;
            _quizRepository = quizRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
        }

        public async Task<QuizAttemptResponseDTO> SubmitAttemptAsync(QuizAttemptCreateDTO dto)
        {
            // Load quiz with details
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(dto.QuizId)
                ?? throw new Exception("Quiz not found.");

            // Check student is enrolled in the course
            if (!await _enrollmentRepository.IsEnrolledAsync(dto.StudentId, quiz.CourseId))
                throw new Exception("You must be enrolled in this course to attempt the quiz.");

            // Check if already attempted
            var existing = await _attemptRepository.GetByStudentAndQuizAsync(dto.StudentId, dto.QuizId);
            if (existing != null)
                throw new Exception("You have already attempted this quiz.");

            // Parse questions from JSON
            var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(quiz.QuestionsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Invalid quiz questions format.");

            // Grade the answers
            int score = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                if (i < dto.Answers.Count &&
                    dto.Answers[i].Trim().Equals(
                        questions[i].CorrectAnswer.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    score++;
                }
            }

            var student = await _userRepository.GetByIdAsync(dto.StudentId)
                ?? throw new Exception("Student not found.");

            // Save the attempt
            var attempt = new QuizAttempt
            {
                QuizId = dto.QuizId,
                Quiz = quiz,
                StudentId = dto.StudentId,
                Student = student,
                Score = score,
                TotalQuestions = questions.Count,
                AnswersJson = JsonSerializer.Serialize(dto.Answers),
                AttemptedAt = DateTime.UtcNow
            };

            await _attemptRepository.AddAsync(attempt);

            return MapToDTO(attempt, quiz, student);
        }

        public async Task<IEnumerable<QuizAttemptResponseDTO>> GetByStudentAsync(int studentId)
        {
            var attempts = await _attemptRepository.GetByStudentAsync(studentId);
            return attempts.Select(a => MapToDTO(a, a.Quiz, a.Student));
        }

        public async Task<IEnumerable<QuizAttemptResponseDTO>> GetByQuizAsync(int quizId)
        {
            var attempts = await _attemptRepository.GetByQuizAsync(quizId);
            return attempts.Select(a => MapToDTO(a, a.Quiz, a.Student));
        }

        public async Task<QuizAttemptResponseDTO> GetByIdAsync(int id)
        {
            var attempt = await _attemptRepository.GetByIdWithDetailsAsync(id)
                ?? throw new Exception("Attempt not found.");
            return MapToDTO(attempt, attempt.Quiz, attempt.Student);
        }

        private static QuizAttemptResponseDTO MapToDTO(QuizAttempt attempt, Quiz quiz, User student)
        {
            return new QuizAttemptResponseDTO
            {
                Id = attempt.Id,
                QuizId = attempt.QuizId,
                QuizTitle = quiz.Title,
                StudentId = attempt.StudentId,
                StudentName = student.Username,
                Score = attempt.Score,
                TotalQuestions = attempt.TotalQuestions,
                Percentage = attempt.TotalQuestions > 0
                    ? Math.Round((double)attempt.Score / attempt.TotalQuestions * 100, 2)
                    : 0,
                AnswersJson = attempt.AnswersJson,
                AttemptedAt = attempt.AttemptedAt
            };
        }
    }

    // Helper class for deserializing quiz questions
    public class QuizQuestion
    {
        public string Question { get; set; } = null!;
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = null!;
    }
}