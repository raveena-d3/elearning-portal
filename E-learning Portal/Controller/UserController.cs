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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly KeycloakAdminService _keycloak;
        private readonly ElearningDbContext _db;

        public UserController(
            IUserService userService,
            KeycloakAdminService keycloak,
            ElearningDbContext db)
        {
            _userService = userService;
            _keycloak = keycloak;
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => Ok(await _userService.GetAllAsync());

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _userService.GetByIdAsync(id)); }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            try
            {
                // 1. Create in database
                var user = await _userService.CreateAsync(dto);

                // 2. Create in Keycloak
                await _keycloak.CreateUserAsync(
                    dto.Username, dto.Password, dto.Role);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(
            int id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                return Ok(await _userService.UpdateRoleAsync(id, request.Role));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get username before deleting
                var user = await _userService.GetByIdAsync(id);

                //  Delete from database
                await _userService.DeleteAsync(id);

                // Delete from Keycloak
                await _keycloak.DeleteUserAsync(user.Username);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = null!;
    }
}