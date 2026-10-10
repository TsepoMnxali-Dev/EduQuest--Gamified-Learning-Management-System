
using EduQuest.API.Data;
using EduQuest.API.DTOs.Auth;
using EduQuest.API.Models.Entities;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly TokenService _tokenService;

        public AuthController(
            ApplicationDBContext context,
            TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ==========================================
        // REGISTER NEW LEARNER
        // ==========================================

        [HttpPost("register")]
        public async Task<ActionResult> Register(
            RegisterDto dto)
        {
            // Normalize email.
            var email = dto.Email.Trim().ToLowerInvariant();

            // Check if email already exists.
            if (await _context.Users.AnyAsync(
                u => u.Email == email))
            {
                return Conflict(
                    "An account with this email already exists.");
            }

            // Get learner role.
            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(
                    r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                return StatusCode(
                    500,
                    "Learner role has not been configured.");
            }

            // Create learner account.
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password),

                RoleID = learnerRole.RoleID,

                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // Existing registration response.
            return Ok(new
            {
                user.UserID,
                user.Email
            });
        }

        // ==========================================
        // LOGIN - ADMIN AND LEARNER
        // ==========================================

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            LoginDto dto)
        {
            // NEW:
            // Basic input validation.
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message =
                        "Please enter your email and password."
                });
            }

            // Normalize email.
            var email = dto.Email.Trim().ToLowerInvariant();

            // Find user and include their role.
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(
                    u => u.Email == email && u.IsActive);

            // ======================================
            // CHANGED: INVALID LOGIN RESPONSE
            // ======================================

            // Same message for invalid email
            // and incorrect password.
            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message =
                        "Incorrect email or password. Please try again."
                });
            }

            // NEW:
            // Ensure the account has a valid role.
            if (user.Role == null ||
                string.IsNullOrWhiteSpace(
                    user.Role.RoleName))
            {
                return StatusCode(500, new
                {
                    message =
                        "Account role is not configured. Contact support."
                });
            }

            // Generate JWT token.
            var token = _tokenService.CreateToken(user);

            // ======================================
            // CHANGED: RETURN USER INFORMATION
            // ======================================

            // The frontend can now identify the
            // correct dashboard without guessing.

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
    }
}
