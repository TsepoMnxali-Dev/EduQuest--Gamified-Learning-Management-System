using EduQuest.API.Data;
using EduQuest.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/leaderboards")]
    [ApiController]
    public class LeaderboardsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LeaderboardsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/leaderboards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaderboardDto>>> GetLeaderboards()
        {
            var leaderboards = await _context.LeaderBoards
                .Select(lb => new LeaderboardDto
                {
                    LeaderboardID = lb.LeaderboardID,
                    TotalPoints = lb.TotalPoints,
                    Rank = lb.Rank,
                    LastUpdated = lb.LastUpdated,
                    LearnerID = lb.LearnerID
                })
                .ToListAsync();

            return Ok(leaderboards);
        }

        // GET: api/leaderboards/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaderboardDto>> GetLeaderboard(int id)
        {
            var leaderboard = await _context.LeaderBoards
                .Where(lb => lb.LeaderboardID == id)
                .Select(lb => new LeaderboardDto
                {
                    LeaderboardID = lb.LeaderboardID,
                    TotalPoints = lb.TotalPoints,
                    Rank = lb.Rank,
                    LastUpdated = lb.LastUpdated,
                    LearnerID = lb.LearnerID
                })
                .FirstOrDefaultAsync();

            if (leaderboard == null)
            {
                return NotFound();
            }

            return Ok(leaderboard);
        }

        // GET: api/leaderboards/grade/{gradeId}
        [HttpGet("grade/{gradeId}")]
        public async Task<ActionResult<IEnumerable<LeaderboardDto>>> GetLeaderboardsByGrade(int gradeId)
        {
            // Layer 2: Make sure the grade actually exists
            var gradeExists = await _context.Grades
                .AnyAsync(grade => grade.GradeID == gradeId);

            if (!gradeExists)
            {
                return NotFound("The specified grade does not exist.");
            }

            // Join through Learner to find leaderboard rows for that grade
            var leaderboards = await _context.LeaderBoards
                .Where(lb => lb.Learner != null && lb.Learner.GradeID == gradeId)
                .Select(lb => new LeaderboardDto
                {
                    LeaderboardID = lb.LeaderboardID,
                    TotalPoints = lb.TotalPoints,
                    Rank = lb.Rank,
                    LastUpdated = lb.LastUpdated,
                    LearnerID = lb.LearnerID
                })
                .ToListAsync();

            return Ok(leaderboards);
        }
    }
}