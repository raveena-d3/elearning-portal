using ElearningAPI.Repositories;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;

namespace ElearningAPI.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        public QuizService(
            IQuizRepository quizRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository)
        {
            _quizRepository = quizRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task<QuizResponseDTO> CreateAsync(QuizCreateDTO dto,
            int requestingUserId, string requestingUserRole)
        {
            var course = await _courseRepository.GetByIdWithInstructorAsync(dto.CourseId)
                ?? throw new Exception("Course not found.");

            // ABAC: Instructor can only post quizzes for their own courses
            if (requestingUserRole == "Instructor" && course.InstructorId != requestingUserId)
                throw new UnauthorizedAccessException("You can only post quizzes for your own courses.");

            var instructor = await _userRepository.GetByIdAsync(requestingUserId)
                ?? throw new Exception("Instructor not found.");

            var quiz = new Quiz
            {
                Title = dto.Title,
                QuestionsJson = dto.QuestionsJson,
                CourseId = dto.CourseId,
                InstructorId = requestingUserId,
                Course = course,
                Instructor = instructor
            };

            await _quizRepository.AddAsync(quiz);
            return quiz.ToDTO();
        }

        public async Task<IEnumerable<QuizResponseDTO>> GetByCourseAsync(int courseId)
        {
            var quizzes = await _quizRepository.GetByCourseAsync(courseId);
            return quizzes.Select(q => q.ToDTO());
        }

        public async Task<QuizResponseDTO> GetByIdAsync(int id)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(id)
                ?? throw new Exception("Quiz not found.");
            return quiz.ToDTO();
        }
        public async Task<IEnumerable<QuizResponseDTO>> GetByInstructorAsync(int instructorId)
        {
            var quizzes = await _quizRepository.GetByInstructorAsync(instructorId);
            return quizzes.Select(q => q.ToDTO());
        }
        public async Task DeleteAsync(int id, int requestingUserId, string requestingUserRole)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(id)
                ?? throw new Exception("Quiz not found.");

            // ABAC: Instructor can only delete quizzes from their own courses
            if (requestingUserRole == "Instructor" && quiz.Course.InstructorId != requestingUserId)
                throw new UnauthorizedAccessException("You can only delete quizzes from your own courses.");

            await _quizRepository.DeleteAsync(quiz);
        }
    }
}