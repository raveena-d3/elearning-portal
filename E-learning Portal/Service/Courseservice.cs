using ElearningAPI.Repositories;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;

namespace ElearningAPI.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        public CourseService(ICourseRepository courseRepository, IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<CourseResponseDTO>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllWithInstructorAsync();
            return courses.Select(c => c.ToDTO());
        }
        public async Task<IEnumerable<CourseResponseDTO>> GetByInstructorAsync(int instructorId)
        {
            var courses = await _courseRepository.GetByInstructorAsync(instructorId);
            return courses.Select(c => c.ToDTO());
        }
        public async Task<CourseResponseDTO> GetByIdAsync(int id)
        {
            var course = await _courseRepository.GetByIdWithInstructorAsync(id)
                ?? throw new Exception("Course not found.");
            return course.ToDTO();
        }

        public async Task<CourseResponseDTO> CreateAsync(CourseCreateDTO dto)
        {
            var instructor = await _userRepository.GetByIdAsync(dto.InstructorId)
                ?? throw new Exception("Instructor not found.");

            if (instructor.Role != Role.Instructor && instructor.Role != Role.Admin)
                throw new Exception("Only Instructors or Admins can be assigned as course instructor.");

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                InstructorId = dto.InstructorId,
                Instructor = instructor,
                YoutubeLinks = dto.YoutubeLinks != null
        ? System.Text.Json.JsonSerializer.Serialize(dto.YoutubeLinks)
        : null
            };

            await _courseRepository.AddAsync(course);
            return course.ToDTO();
        }

        public async Task<CourseResponseDTO> UpdateAsync(int id, CourseCreateDTO dto,
            int requestingUserId, string requestingUserRole)
        {
            var course = await _courseRepository.GetByIdWithInstructorAsync(id)
                ?? throw new Exception("Course not found.");

            // ABAC: Instructor can only update their own courses
            if (requestingUserRole == "Instructor" && course.InstructorId != requestingUserId)
                throw new UnauthorizedAccessException("You can only update your own courses.");

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.YoutubeLinks = dto.YoutubeLinks != null
    ? System.Text.Json.JsonSerializer.Serialize(dto.YoutubeLinks)
    : course.YoutubeLinks;
            await _courseRepository.UpdateAsync(course);
            return course.ToDTO();
        }

        public async Task DeleteAsync(int id, int requestingUserId, string requestingUserRole)
        {
            var course = await _courseRepository.GetByIdAsync(id)
                ?? throw new Exception("Course not found.");

            // ABAC: Instructor can only delete their own courses
            if (requestingUserRole == "Instructor" && course.InstructorId != requestingUserId)
                throw new UnauthorizedAccessException("You can only delete your own courses.");

            await _courseRepository.DeleteAsync(course);
        }
        public async Task<CourseResponseDTO> UpdateVideoAsync(int id, string fileName, string originalName, int userId, string role)
        {
            var course = await _courseRepository.GetByIdWithInstructorAsync(id)
                ?? throw new Exception("Course not found.");

            if (role == "Instructor" && course.InstructorId != userId)
                throw new UnauthorizedAccessException("You can only upload videos for your own courses.");

            course.VideoFileName = fileName;
            course.VideoOriginalName = originalName;  
            await _courseRepository.UpdateAsync(course);
            return course.ToDTO();
        }
        public async Task ClearVideoAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id)
                ?? throw new Exception("Course not found.");
            course.VideoFileName = null;
            course.VideoOriginalName = null;
            await _courseRepository.UpdateAsync(course);
        }
    }
}