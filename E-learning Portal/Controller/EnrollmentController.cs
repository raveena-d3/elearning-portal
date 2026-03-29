using ElearningAPI.Services;
using ElearningAPI.Helpers;
using ElearningAPI.Data;
using E_learning_Portal.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElearningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ElearningDbContext _db;

        public EnrollmentController(
            IEnrollmentService enrollmentService, ElearningDbContext db)
        {
            _enrollmentService = enrollmentService;
            _db = db;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                dto.StudentId = userId;
                return Ok(await _enrollmentService.EnrollAsync(dto));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            var role = KeycloakClaimsHelper.GetRole(User);
            if (role == "Student" && studentId != userId) return Forbid();
            return Ok(await _enrollmentService.GetByStudentAsync(studentId));
        }
        [HttpGet("check/{courseId}")]
        [Authorize(Roles ="Student")]
        public async Task<IActionResult> IsEnrolled(int courseId)
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            var enrolled = await _enrollmentService.IsEnrolledAsync(userId, courseId);
            return Ok(new { isEnrolled = enrolled });
        }
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            if (userId == 0) return Unauthorized();
            return Ok(await _enrollmentService.GetByStudentAsync(userId));
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(await _enrollmentService.GetByCourseAsync(courseId));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> Unenroll(int id)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                await _enrollmentService.UnenrollAsync(id, userId, role);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}