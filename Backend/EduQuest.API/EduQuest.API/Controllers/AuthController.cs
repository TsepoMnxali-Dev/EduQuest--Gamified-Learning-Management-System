using EduQuest.API.Data;
using EduQuest.API.DTOs.Auth;
using EduQuest.API.Models.Entities;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly TokenService _tokenService;

        public AuthController(ApplicationDBContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict("An account with this email already exists.");

            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Learner");

            if (learnerRole == null)
                return StatusCode(500, "Learner role has not been configured.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID = learnerRole.RoleID,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.UserID, user.Email });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Your current password is incorrect.");

            if (dto.NewPassword.Length < 8)
                return BadRequest("The new password must be at least 8 characters.");

            if (dto.NewPassword == dto.CurrentPassword)
                return BadRequest("The new password must be different from your current password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/auth/register-learner
        // Creates the user, the learner profile and the subject enrolments
        // in ONE transaction, then returns a token so the learner is signed in.
        [HttpPost("register-learner")]
        public async Task<ActionResult> RegisterLearner(RegisterLearnerDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var subjectIds = dto.SubjectIDs.Distinct().ToList();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict("An account with this email already exists.");

            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Learner");
            if (learnerRole == null)
                return StatusCode(500, "Learner role has not been configured.");

            if (!await _context.Grades.AnyAsync(g => g.GradeID == dto.GradeID))
                return NotFound("Grade not found.");

            if (!await _context.Schools.AnyAsync(s => s.SchoolID == dto.SchoolID))
                return NotFound("School not found.");

            // Every chosen subject must be offered for the chosen grade.
            var offeredCount = await _context.GradeSubjects
                .CountAsync(gs => gs.GradeID == dto.GradeID &&
                                  subjectIds.Contains(gs.SubjectID));
            if (offeredCount != subjectIds.Count)
                return BadRequest("One or more subjects are not offered for the selected grade.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleID = learnerRole.RoleID,
                    IsActive = true,
                    DateCreated = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var learner = new Learner
                {
                    UserID = user.UserID,
                    GradeID = dto.GradeID,
                    SchoolID = dto.SchoolID
                };
                _context.Learners.Add(learner);
                await _context.SaveChangesAsync();

                foreach (var subjectId in subjectIds)
                {
                    _context.learnerSubjects.Add(new LearnerSubject
                    {
                        LearnerID = learner.LearnerID,
                        SubjectID = subjectId
                    });
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                user.Role = learnerRole;
                return Ok(new { token = _tokenService.CreateToken(user) });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return Conflict("Could not create the account. If you already tried, the email may be taken.");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var token = _tokenService.CreateToken(user);

            return Ok(new { token });
        }
    }
}
