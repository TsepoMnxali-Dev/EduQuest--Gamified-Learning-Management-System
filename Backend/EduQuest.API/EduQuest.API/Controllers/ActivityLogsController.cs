using EduQuest.API.Data;
using EduQuest.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ActivityLogsController(ApplicationDBContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetActivityLogs()
        {
            var logs = await _context.activityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.DateTime)
                .Select(a => new ActivityLogDto
                {
                    ActivityLogID = a.ActivityLogID,
                    UserID = a.UserID,
                    UserFullName = a.User.FirstName + " " + a.User.LastName,
                    Action = a.Action,
                    DateTime = a.DateTime,
                    IPAddress = a.IPAddress
                })
                .ToListAsync();

            return Ok(logs);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityLogDto>> GetActivityLog(int id)
        {
            var log = await _context.activityLogs
                .Include(a => a.User)
                .Where(a => a.ActivityLogID == id)
                .Select(a => new ActivityLogDto
                {
                    ActivityLogID = a.ActivityLogID,
                    UserID = a.UserID,
                    UserFullName = a.User.FirstName + " " + a.User.LastName,
                    Action = a.Action,
                    DateTime = a.DateTime,
                    IPAddress = a.IPAddress
                })
                .FirstOrDefaultAsync();

            if (log == null)
                return NotFound();

            return Ok(log);
        }
    }
}