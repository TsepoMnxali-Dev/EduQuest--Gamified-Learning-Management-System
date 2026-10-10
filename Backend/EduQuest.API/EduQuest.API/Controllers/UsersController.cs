using System.Security.Claims;
using EduQuest.API.Data;
using EduQuest.API.DTOs.Users;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UsersController(ApplicationDBContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RoleID = u.RoleID,
                    RoleName = u.Role!.RoleName,
                    IsActive = u.IsActive,
                    DateCreated = u.DateCreated
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserID == id)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RoleID = u.RoleID,
                    RoleName = u.Role!.RoleName,
                    IsActive = u.IsActive,
                    DateCreated = u.DateCreated
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: (admin-created accounts — for normal signup, use AuthController.Register)
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict("An account with this email already exists.");

            var role = await _context.Roles.FindAsync(dto.RoleID);

            if (role == null)
                return NotFound("Role not found.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleID = dto.RoleID,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = role.RoleName,
                IsActive = user.IsActive,
                DateCreated = user.DateCreated
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, result);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return NotFound();

            // Admin accounts can't be suspended through the API.
            if (dto.IsActive == false && user.Role?.RoleName == "Admin")
                return StatusCode(StatusCodes.Status403Forbidden, "Admin accounts cannot be deactivated.");

            var email = dto.Email.Trim().ToLowerInvariant();

            // An admin may change their own email, but not another admin's.
            var isCurrentUser = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId)
                                && currentUserId == user.UserID;

            if (user.Role?.RoleName == "Admin" && !isCurrentUser
                && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                return StatusCode(StatusCodes.Status403Forbidden, "You cannot change another admin's email.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email && u.UserID != id);

            if (emailExists)
                return Conflict("An account with this email already exists.");

            var role = await _context.Roles.FindAsync(dto.RoleID);

            if (role == null)
                return NotFound("Role not found.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = email;
            user.RoleID = dto.RoleID;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/{id}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return NotFound();

            // Admin accounts can't be deactivated, including by other admins.
            if (user.Role?.RoleName == "Admin")
                return StatusCode(StatusCodes.Status403Forbidden, "Admin accounts cannot be deactivated.");

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}