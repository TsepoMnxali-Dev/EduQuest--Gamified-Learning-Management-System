using System.Security.Claims;
using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        public ProfileController(ApplicationDBContext db) => _db = db;

        public sealed class UpdateMyProfileRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(100)]
            public string FirstName { get; set; } = string.Empty;
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(100)]
            public string LastName { get; set; } = string.Empty;
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.EmailAddress]
            [System.ComponentModel.DataAnnotations.StringLength(256)]
            public string Email { get; set; } = string.Empty;
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMyProfileRequest request)
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(rawId, out var userId)) return Unauthorized();
            var account = await _db.Users.FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive);
            if (account == null) return Unauthorized();
            var firstName = request.FirstName?.Trim() ?? "";
            var lastName = request.LastName?.Trim() ?? "";
            var email = request.Email?.Trim().ToLowerInvariant() ?? "";
            if (firstName.Length == 0 || lastName.Length == 0 || email.Length == 0)
                return BadRequest(new { message = "Name and email are required." });
            if (await _db.Users.AnyAsync(u => u.UserID != userId && u.Email == email))
                return Conflict(new { message = "An account with this email already exists." });
            account.FirstName = firstName;
            account.LastName = lastName;
            account.Email = email;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(rawId, out var userId)) return Unauthorized();

            var account = await _db.Users.AsNoTracking()
                .Where(u => u.UserID == userId && u.IsActive)
                .Select(u => new {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.IsActive,
                    u.DateCreated,
                    Role = u.Role!.RoleName
                })
                .FirstOrDefaultAsync();
            if (account == null) return Unauthorized();

            var learner = await _db.Learners.AsNoTracking()
                .Where(l => l.UserID == userId)
                .Select(l => new {
                    l.LearnerID,
                    Grade = l.Grade.GradeName,
                    School = l.School.SchoolName
                }).FirstOrDefaultAsync();

            var subjects = learner == null ? new List<string>() : await _db.learnerSubjects
                .AsNoTracking().Where(s => s.LearnerID == learner.LearnerID)
                .Select(s => s.Subject.SubjectName).Distinct().OrderBy(s => s).ToListAsync();

            return Ok(new
            {
                account.UserID,
                account.FirstName,
                account.LastName,
                account.Email,
                account.IsActive,
                account.DateCreated,
                account.Role,
                LearnerID = learner?.LearnerID,
                Grade = learner?.Grade,
                School = learner?.School,
                Subjects = subjects
            });
        }
    }
}
