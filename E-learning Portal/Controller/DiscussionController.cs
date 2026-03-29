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
    public class DiscussionController : ControllerBase
    {
        private readonly IDiscussionService _discussionService;
        private readonly ElearningDbContext _db;

        public DiscussionController(
            IDiscussionService discussionService, ElearningDbContext db)
        {
            _discussionService = discussionService;
            _db = db;
        }
        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCoursesDiscussions()
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var discussions = await _discussionService.GetByInstructorAsync(userId);
                return Ok(discussions);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AskQuestion([FromBody] DiscussionCreateDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                dto.StudentId = userId;
                return Ok(await _discussionService.AskQuestionAsync(dto));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}/answer")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> AnswerQuestion(
            int id, [FromBody] AnswerDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                return Ok(await _discussionService
                    .AnswerQuestionAsync(id, dto.Answer, userId, role));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(await _discussionService.GetByCourseAsync(courseId));

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            var role = KeycloakClaimsHelper.GetRole(User);
            if (role == "Student" && studentId != userId) return Forbid();
            return Ok(await _discussionService.GetByStudentAsync(studentId));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyDiscussions()
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            if (userId == 0) return Unauthorized();
            return Ok(await _discussionService.GetByStudentAsync(userId));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                await _discussionService.DeleteAsync(id, userId, role);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }

    public class AnswerDTO
    {
        public string Answer { get; set; } = null!;
    }
}