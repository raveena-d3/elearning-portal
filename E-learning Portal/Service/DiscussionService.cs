using ElearningAPI.Repositories;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;

namespace ElearningAPI.Services
{
    public class DiscussionService : IDiscussionService
    {
        private readonly IDiscussionRepository _discussionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;

        public DiscussionService(
            IDiscussionRepository discussionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository)
        {
            _discussionRepository = discussionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
        }

        public async Task<DiscussionResponseDTO> AskQuestionAsync(DiscussionCreateDTO dto)
        {
            var course = await _courseRepository.GetByIdWithInstructorAsync(dto.CourseId)
                ?? throw new Exception("Course not found.");

            // Student must be enrolled before they can ask questions
            if (!await _enrollmentRepository.IsEnrolledAsync(dto.StudentId, dto.CourseId))
                throw new Exception("Student must be enrolled in the course to ask questions.");

            var discussion = new Discussion
            {
                CourseId = dto.CourseId,
                Course = course,
                StudentId = dto.StudentId,
                Question = dto.Question,
                AskedAt = DateTime.UtcNow
            };

            await _discussionRepository.AddAsync(discussion);

            // Reload with Student navigation property for DTO mapping
            var saved = await _discussionRepository.GetByIdWithDetailsAsync(discussion.Id);
            return saved!.ToDTO();
        }

        public async Task<DiscussionResponseDTO> AnswerQuestionAsync(int discussionId,
            string answer, int requestingUserId, string requestingUserRole)
        {
            var discussion = await _discussionRepository.GetByIdWithDetailsAsync(discussionId)
                ?? throw new Exception("Discussion not found.");

            // ABAC: Instructor can only answer in their own courses
            if (requestingUserRole == "Instructor"
                && discussion.Course.InstructorId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "You can only answer questions in your own courses.");

            discussion.Answer = answer;
            discussion.AnsweredAt = DateTime.UtcNow;

            await _discussionRepository.UpdateAsync(discussion);
            return discussion.ToDTO();
        }

        public async Task<IEnumerable<DiscussionResponseDTO>> GetByCourseAsync(int courseId)
        {
            var discussions = await _discussionRepository.GetByCourseAsync(courseId);
            return discussions.Select(d => d.ToDTO());
        }

        public async Task<IEnumerable<DiscussionResponseDTO>> GetByStudentAsync(int studentId)
        {
            var discussions = await _discussionRepository.GetByStudentAsync(studentId);
            return discussions.Select(d => d.ToDTO());
        }
        public async Task<IEnumerable<DiscussionResponseDTO>> GetByInstructorAsync(int instructorId)
        {
            // Get all courses owned by this instructor
            var courses = await _courseRepository.GetByInstructorAsync(instructorId);
            var allDiscussions = new List<DiscussionResponseDTO>();

            foreach (var course in courses)
            {
                var discussions = await _discussionRepository.GetByCourseAsync(course.Id);
                allDiscussions.AddRange(discussions.Select(d => d.ToDTO()));
            }

            return allDiscussions.OrderByDescending(d => d.AskedAt);
        }
        public async Task DeleteAsync(int id, int requestingUserId, string requestingUserRole)
        {
            var discussion = await _discussionRepository.GetByIdAsync(id)
                ?? throw new Exception("Discussion not found.");

            // ABAC: Student can only delete their own questions
            if (requestingUserRole == "Student" && discussion.StudentId != requestingUserId)
                throw new UnauthorizedAccessException("You can only delete your own questions.");

            await _discussionRepository.DeleteAsync(discussion);
        }
    }
}