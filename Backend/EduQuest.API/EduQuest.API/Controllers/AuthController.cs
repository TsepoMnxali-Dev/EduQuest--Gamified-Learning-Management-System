using System.Security.Cryptography;
using System.Text;
using EduQuest.API.Data;
using EduQuest.API.DTOs.Auth;
using EduQuest.API.Models.Entities;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDBContext context,
            TokenService tokenService,
            IEmailService emailService,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        // ==========================================
        // EXISTING: BASIC LEARNER REGISTRATION
        // ==========================================

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict("An account with this email already exists.");
            }

            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                return StatusCode(500, "Learner role has not been configured.");
            }

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

            return Ok(new
            {
                user.UserID,
                user.Email
            });
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

        // ==========================================
        // NEW: COMPLETE LEARNER REGISTRATION
        // POST: api/auth/register-learner
        // ==========================================

        [HttpPost("register-learner")]
        public async Task<ActionResult> RegisterLearner(RegisterLearnerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Please complete all account details." });
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new { message = "An account with this email already exists." });
            }

            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                return StatusCode(500, new { message = "Learner role is not configured." });
            }

            var gradeExists = await _context.Grades
                .AnyAsync(g => g.GradeID == dto.GradeID);

            if (!gradeExists)
            {
                return BadRequest(new { message = "The selected grade does not exist." });
            }

            var schoolExists = await _context.Schools
                .AnyAsync(s => s.SchoolID == dto.SchoolID);

            if (!schoolExists)
            {
                return BadRequest(new { message = "The selected school does not exist." });
            }

            if (dto.SubjectIDs == null || dto.SubjectIDs.Count == 0)
            {
                return BadRequest(new { message = "Please select at least one subject." });
            }

            var subjectIds = dto.SubjectIDs.Distinct().ToList();

            if (subjectIds.Any(id => id <= 0))
            {
                return BadRequest(new { message = "One or more selected subjects are invalid." });
            }

            var validSubjectCount = await _context.Subjects
                .CountAsync(s => subjectIds.Contains(s.SubjectID));

            if (validSubjectCount != subjectIds.Count)
            {
                return BadRequest(new { message = "One or more selected subjects do not exist." });
            }

            var validGradeSubjectCount = await _context.GradeSubjects.CountAsync(
                gs => gs.GradeID == dto.GradeID && subjectIds.Contains(gs.SubjectID));

            if (validGradeSubjectCount != subjectIds.Count)
            {
                return BadRequest(new { message = "One or more subjects are not available for the selected grade." });
            }

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
                    Role = learnerRole,
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

                var token = _tokenService.CreateToken(user);

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Learner account created successfully.",
                    token = token,
                    user = new
                    {
                        userId = user.UserID,
                        learnerId = learner.LearnerID,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        role = learnerRole.RoleName
                    }
                });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Unable to save your learner account. Please try again." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An unexpected error occurred during registration." });
            }
        }

        // ==========================================
        // EXISTING: ADMIN AND LEARNER LOGIN
        // ==========================================

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Please enter your email and password." });
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Incorrect email or password. Please try again." });
            }

            if (user.Role == null || string.IsNullOrWhiteSpace(user.Role.RoleName))
            {
                return StatusCode(500, new { message = "Account role is not configured. Contact support." });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                user = new
                {
                    userId = user.UserID,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role.RoleName
                }
            });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            const string genericMessage =
                "If an account exists for that email, a password reset link has been sent.";

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
                return Ok(new { message = genericMessage });

            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            user.PasswordResetTokenHash = HashToken(token);
            user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendBase = (_config["Frontend:BaseUrl"] ?? "http://127.0.0.1:5500").TrimEnd('/');
            var link = $"{frontendBase}/reset-password.html" +
                       $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            var body =
                $"<p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>" +
                "<p>We received a request to reset your EduQuest password. " +
                "Click the link below to choose a new one. It expires in 1 hour.</p>" +
                $"<p><a href=\"{link}\">Reset my password</a></p>" +
                "<p>If you did not ask for this, you can ignore this email.</p>";

            try
            {
                await _emailService.SendAsync(email, "Reset your EduQuest password", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            }

            return Ok(new { message = genericMessage });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto dto)
        {
            const string invalidMessage =
                "This reset link is invalid or has expired. Please request a new one.";

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null ||
                string.IsNullOrEmpty(user.PasswordResetTokenHash) ||
                user.PasswordResetExpiresAt == null ||
                user.PasswordResetExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(invalidMessage);
            }

            var suppliedHash = Encoding.UTF8.GetBytes(HashToken(dto.Token));
            var storedHash = Encoding.UTF8.GetBytes(user.PasswordResetTokenHash);

            if (!CryptographicOperations.FixedTimeEquals(suppliedHash, storedHash))
                return BadRequest(invalidMessage);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            user.PasswordResetTokenHash = null;
            user.PasswordResetExpiresAt = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Your password has been reset. You can now sign in." });
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}