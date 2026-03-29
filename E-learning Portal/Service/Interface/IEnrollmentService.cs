using E_learning_Portal.Dto;

namespace ElearningAPI.Services
{
    public interface IEnrollmentService
    {
        Task<EnrollmentResponseDTO> EnrollAsync(EnrollmentCreateDTO dto);
        Task<IEnumerable<EnrollmentResponseDTO>> GetByStudentAsync(int studentId);
        Task<IEnumerable<EnrollmentResponseDTO>> GetByCourseAsync(int courseId);
        Task UnenrollAsync(int enrollmentId, int requestingUserId, string requestingUserRole);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
    }
}