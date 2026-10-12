using System.Security.Claims;
using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    // Help & Support: learners send requests and read replies; admins answer them.
    [Authorize]
    [ApiController]
    [Route("api/support")]
    public class SupportController : ControllerBase
    {
        private static readonly string[] AllowedCategories =
        {
            "Account", "Quizzes", "Classes & Subjects", "Technical Problem", "Other"
        };

        private static readonly string[] AllowedStatuses =
        {
            "Open", "In Progress", "Resolved"
        };

        // Stops one account from flooding the inbox.
        private const int MaxRequestsPerHour = 5;

        private readonly ApplicationDBContext _context;

        public SupportController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST: api/support
        [HttpPost]
        public async Task<ActionResult<SupportRequestDto>> CreateRequest(CreateSupportRequestDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var category = AllowedCategories.FirstOrDefault(c =>
                string.Equals(c, dto.Category.Trim(), StringComparison.OrdinalIgnoreCase));
            if (category == null)
                return BadRequest("Please choose a valid category.");

            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var recentCount = await _context.SupportRequests
                .CountAsync(r => r.UserID == userId && r.DateCreated >= oneHourAgo);
            if (recentCount >= MaxRequestsPerHour)
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    "You have sent several requests recently. Please wait a while before sending another.");

            var request = new SupportRequest
            {
                UserID = userId,
                Category = category,
                Subject = dto.Subject.Trim(),
                Message = dto.Message.Trim(),
                Status = "Open",
                DateCreated = DateTime.UtcNow
            };

            _context.SupportRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(ToDto(request));
        }

        // GET: api/support/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetMyRequests()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var requests = await _context.SupportRequests
                .Where(r => r.UserID == userId)
                .OrderByDescending(r => r.DateCreated)
                .Take(20)
                .ToListAsync();

            return Ok(requests.Select(ToDto));
        }

        // GET: api/support?status=Open   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminSupportRequestDto>>> GetAllRequests(
            [FromQuery] string? status = null)
        {
            var query = _context.SupportRequests.Include(r => r.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var wanted = status.Trim();
                query = query.Where(r => r.Status == wanted);
            }

            var requests = await query
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();

            return Ok(requests.Select(r => new AdminSupportRequestDto
            {
                SupportRequestID = r.SupportRequestID,
                Category = r.Category,
                Subject = r.Subject,
                Message = r.Message,
                Status = r.Status,
                DateCreated = AsUtc(r.DateCreated),
                AdminReply = r.AdminReply,
                DateReplied = r.DateReplied.HasValue ? AsUtc(r.DateReplied.Value) : null,
                UserID = r.UserID,
                FullName = r.User == null ? "" : r.User.FirstName + " " + r.User.LastName,
                Email = r.User == null ? "" : r.User.Email
            }));
        }

        // PUT: api/support/{id}   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRequest(int id, UpdateSupportRequestDto dto)
        {
            var request = await _context.SupportRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            var status = AllowedStatuses.FirstOrDefault(s =>
                string.Equals(s, dto.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            if (status == null)
                return BadRequest("Status must be Open, In Progress or Resolved.");

            request.Status = status;

            var reply = string.IsNullOrWhiteSpace(dto.AdminReply) ? null : dto.AdminReply.Trim();
            if (reply != request.AdminReply)
            {
                request.AdminReply = reply;
                request.DateReplied = reply == null ? null : DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Dates are stored in UTC; marking the kind makes the JSON end in "Z"
        // so the browser shows the learner's local time.
        private static DateTime AsUtc(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static SupportRequestDto ToDto(SupportRequest r)
        {
            return new SupportRequestDto
            {
                SupportRequestID = r.SupportRequestID,
                Category = r.Category,
                Subject = r.Subject,
                Message = r.Message,
                Status = r.Status,
                DateCreated = AsUtc(r.DateCreated),
                AdminReply = r.AdminReply,
                DateReplied = r.DateReplied.HasValue ? AsUtc(r.DateReplied.Value) : null
            };
        }
    }
}