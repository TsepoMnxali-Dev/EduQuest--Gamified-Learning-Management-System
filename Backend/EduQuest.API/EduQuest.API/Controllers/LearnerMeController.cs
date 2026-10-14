using System.Security.Claims;
using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    // Just enough about the signed-in learner to show in the top corner of a page.
    [Authorize]
    [ApiController]
    [Route("api/learner/me")]
    public class LearnerMeController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LearnerMeController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/learner/me
        [HttpGet]
        public async Task<ActionResult<LearnerMeDto>> GetMe()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var me = await _context.Learners
                .Where(l => l.UserID == userId)
                .Select(l => new LearnerMeDto
                {
                    FirstName = l.User!.FirstName,
                    LastName = l.User.LastName,
                    GradeName = l.Grade!.GradeName
                })
                .FirstOrDefaultAsync();

            if (me == null)
                return NotFound("No learner profile exists for this account.");

            return Ok(me);
        }
    }

    public class LearnerMeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
    }
}