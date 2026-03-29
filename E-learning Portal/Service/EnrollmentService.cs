using ElearningAPI.Repositories;
using E_learning_Portal.Dto;
using E_learning_Portal.Helpers;
using E_learning_Portal.models;

namespace ElearningAPI.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task<EnrollmentResponseDTO> EnrollAsync(EnrollmentCreateDTO dto)
        {
            var course = await _courseRepository.GetByIdAsync(dto.CourseId)
                ?? throw new Exception("Course not found.");

            var student = await _userRepository.GetByIdAsync(dto.StudentId)
                ?? throw new Exception("Student not found.");

            if (student.Role != Role.Student)
                throw new Exception("Only Students can enroll in courses.");

            if (await _enrollmentRepository.IsEnrolledAsync(dto.StudentId, dto.CourseId))
                throw new Exception("Student is already enrolled in this course.");

            var enrollment = new Enrollment
            {
                CourseId = dto.CourseId,
                StudentId = dto.StudentId,
                Course = course,
                Student = student,
                EnrolledAt = DateTime.UtcNow
            };

            await _enrollmentRepository.AddAsync(enrollment);
            return enrollment.ToDTO();
        }

        public async Task<IEnumerable<EnrollmentResponseDTO>> GetByStudentAsync(int studentId)
        {
            var enrollments = await _enrollmentRepository.GetByStudentAsync(studentId);
            return enrollments.Select(e => e.ToDTO());
        }

        public async Task<IEnumerable<EnrollmentResponseDTO>> GetByCourseAsync(int courseId)
        {
            var enrollments = await _enrollmentRepository.GetByCourseAsync(courseId);
            return enrollments.Select(e => e.ToDTO());
        }

        public async Task UnenrollAsync(int enrollmentId, int requestingUserId, string requestingUserRole)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId)
                ?? throw new Exception("Enrollment not found.");

            // ABAC: Students can only unenroll themselves
            if (requestingUserRole == "Student" && enrollment.StudentId != requestingUserId)
                throw new UnauthorizedAccessException("You can only unenroll yourself.");

            await _enrollmentRepository.DeleteAsync(enrollment);
        }
        public async Task<bool>IsEnrolledAsync(int studentId,int courseId)
        {
            return await _enrollmentRepository.IsEnrolledAsync(studentId, courseId);
        }
    }
}