using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElearningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        [HttpGet("claims")]
        [Authorize]
        public IActionResult GetClaims()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity!.IsAuthenticated,
                Name = User.Identity.Name,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("admin-test")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminTest() => Ok("Admin role works!");

        [HttpGet("instructor-test")]
        [Authorize(Roles = "Instructor")]
        public IActionResult InstructorTest() => Ok("Instructor role works!");

        [HttpGet("student-test")]
        [Authorize(Roles = "Student")]
        public IActionResult StudentTest() => Ok("Student role works!");
    }
}