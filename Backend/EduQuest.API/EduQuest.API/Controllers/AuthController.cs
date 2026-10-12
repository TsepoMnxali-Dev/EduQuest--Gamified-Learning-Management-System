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
                IsActive = true
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