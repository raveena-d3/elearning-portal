using E_learning_Portal.Dto;
using E_learning_Portal.Service.Interface;
using ElearningAPI.Helpers;
using ElearningAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElearningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;
        private readonly ElearningDbContext _db;

        public QuizAttemptController(
            IQuizAttemptService attemptService, ElearningDbContext db)
        {
            _attemptService = attemptService;
            _db = db;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit([FromBody] QuizAttemptCreateDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                dto.StudentId = userId;
                var result = await _attemptService.SubmitAttemptAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            var role = KeycloakClaimsHelper.GetRole(User);
            if (role == "Student" && studentId != userId) return Forbid();
            return Ok(await _attemptService.GetByStudentAsync(studentId));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyAttempts()
        {
            var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
            if (userId == 0) return Unauthorized();
            return Ok(await _attemptService.GetByStudentAsync(userId));
        }

        [HttpGet("quiz/{quizId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByQuiz(int quizId)
            => Ok(await _attemptService.GetByQuizAsync(quizId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _attemptService.GetByIdAsync(id)); }
            catch (Exception ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}