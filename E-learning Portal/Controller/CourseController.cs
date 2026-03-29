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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ElearningDbContext _db;

        public CourseController(ICourseService courseService, ElearningDbContext db)
        {
            _courseService = courseService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _courseService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _courseService.GetByIdAsync(id)); }
            catch (Exception ex) { return NotFound(new { message = ex.Message }); }
        }
        // ★ NEW — Get courses for logged in instructor only
        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var courses = await _courseService.GetByInstructorAsync(userId);
                return Ok(courses);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CourseCreateDTO dto)
        {
            try
            {
                var course = await _courseService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseCreateDTO dto)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                return Ok(await _courseService.UpdateAsync(id, dto, userId, role));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);
                await _courseService.DeleteAsync(id, userId, role);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpPost("{id}/upload-video")]
        [Authorize(Roles = "Admin,Instructor")]
        [RequestSizeLimit(524288000)]          // 
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> UploadVideo(int id, IFormFile file)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded." });

                var allowedTypes = new[] { "video/mp4", "video/webm", "video/ogg" };
                if (!allowedTypes.Contains(file.ContentType))
                    return BadRequest(new { message = "Only MP4, WebM, OGG video files are allowed." });

                if (file.Length > 524288000)
                    return BadRequest(new { message = "File size cannot exceed 500MB." });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "videos");
                Directory.CreateDirectory(uploadsFolder);

                // ★ DELETE OLD VIDEO FILE IF EXISTS
                var existingCourse = await _courseService.GetByIdAsync(id);
                if (!string.IsNullOrEmpty(existingCourse.VideoFileName))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, existingCourse.VideoFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);  // Delete old file from disk
                    }
                }

                // Save new file
                var originalName = file.FileName;
                var fileName = $"course_{id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save new filename to database
                var result = await _courseService.UpdateVideoAsync(id, fileName,originalName, userId, role);
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpGet("{id}/video")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideo(int id)
        {
            try
            {
                var course = await _courseService.GetByIdAsync(id);
                if (string.IsNullOrEmpty(course.VideoFileName))
                    return NotFound(new { message = "No video uploaded." });

                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "uploads", "videos", course.VideoFileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Video file not found." });

                var ext = Path.GetExtension(course.VideoFileName).ToLower();
                var mimeType = ext switch
                {
                    ".mp4" => "video/mp4",
                    ".webm" => "video/webm",
                    ".ogg" => "video/ogg",
                    _ => "video/mp4"
                };

                // PhysicalFile releases file handle after streaming
                // enableRangeProcessing allows video seeking in browser
                return PhysicalFile(filePath, mimeType, enableRangeProcessing: true);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── Delete Video ───────────────────────────────────────────────────────
        [HttpDelete("{id}/video")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            try
            {
                var userId = await KeycloakClaimsHelper.GetUserIdAsync(User, _db);
                var role = KeycloakClaimsHelper.GetRole(User);

                var course = await _courseService.GetByIdAsync(id);

                // ABAC — Instructor can only delete video from their own course
                if (role == "Instructor" && course.InstructorId != userId)
                    return Forbid();

                // ★ Only clear from database — do NOT try to delete the file
                // File is locked by Windows while being streamed
                // It will be cleaned up next time a new video is uploaded
                await _courseService.ClearVideoAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}