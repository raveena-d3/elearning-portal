using ElearningAPI.Services;
using ElearningAPI.Helpers;
using ElearningAPI.Data;
using E_learning_Portal.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElearningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ElearningDbContext _db;

        public QuizController(IQuizService quizService, ElearningDbContext db)
        {
            _quizService = quizService;
            _db = db;
        }
        // ★ NEW — Get quizzes for instructor's own courses only
        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCourseQuizzes()
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var quizzes = await _quizService.GetByInstructorAsync(userId);
                return Ok(quizzes);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create([FromBody] QuizCreateDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                var quiz = await _quizService.CreateAsync(dto, userId, role);
                return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(await _quizService.GetByCourseAsync(courseId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _quizService.GetByIdAsync(id)); }
            catch (Exception ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                await _quizService.DeleteAsync(id, userId, role);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}