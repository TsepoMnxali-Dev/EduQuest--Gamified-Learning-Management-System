using System.Security.Claims;
using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/achievement-badges")]
    public class AchievementBadgesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AchievementBadgesController(ApplicationDBContext context)
        {
            _context = context;
        }

        private async Task<int?> CurrentLearnerIdAsync()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claim, out var userId))
                return null;

            return await _context.Learners.AsNoTracking()
                .Where(l => l.UserID == userId)
                .Select(l => (int?)l.LearnerID)
                .FirstOrDefaultAsync();
        }

        // GET /api/achievement-badges/me
        [Authorize(Roles = "Learner")]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<AchievementBadgeDto>>> GetMine()
        {
            var learnerId = await CurrentLearnerIdAsync();
            if (!learnerId.HasValue)
                return NotFound("Learner profile was not found for this login.");

            await AchievementAwardService.SyncForLearnerAsync(
                _context, learnerId.Value);

            var badges = await _context.LearnerAchievements
                .AsNoTracking()
                .Where(la => la.LearnerID == learnerId.Value)
                .Select(la => new AchievementBadgeDto
                {
                    AchievementID = la.AchievementID,
                    Name = la.Achievement!.Name,
                    Description = la.Achievement.Description,
                    BadgeImage = la.Achievement.BadgeImage,
                    PointsRequired = la.Achievement.PointsRequired
                })
                .OrderBy(b => b.AchievementID)
                .ToListAsync();

            return Ok(badges);
        }

        // GET /api/achievement-badges/me/diagnostics
        // Learner-only, reports counts for their own account (no private data).
        [Authorize(Roles = "Learner")]
        [HttpGet("me/diagnostics")]
        public async Task<IActionResult> GetMyDiagnostics()
        {
            var learnerId = await CurrentLearnerIdAsync();
            if (!learnerId.HasValue)
                return NotFound("Learner profile was not found for this login.");

            var attemptsStarted = await _context.QuizAttempts
                .CountAsync(a => a.LearnerID == learnerId.Value);

            var attemptsSubmitted = await _context.QuizAttempts
                .CountAsync(a => a.LearnerID == learnerId.Value &&
                    _context.QuizAttemptAnswers.Any(answer =>
                        answer.AttemptID == a.QuizAttemptID));

            var awardedBadges = await _context.LearnerAchievements
                .CountAsync(la => la.LearnerID == learnerId.Value);

            return Ok(new
            {
                attemptsStarted,
                attemptsSubmitted,
                awardedBadges
            });
        }

        // POST /api/achievement-badges/backfill
        // Only submitted attempts (with saved answers) are considered.
        [Authorize(Roles = "Admin")]
        [HttpPost("backfill")]
        public async Task<IActionResult> Backfill()
        {
            var attemptsStarted = await _context.QuizAttempts.CountAsync();

            var submittedAttempts = await _context.QuizAttempts
                .CountAsync(a => _context.QuizAttemptAnswers.Any(answer =>
                    answer.AttemptID == a.QuizAttemptID));

            var learnerIds = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => _context.QuizAttemptAnswers.Any(answer =>
                    answer.AttemptID == a.QuizAttemptID))
                .Select(a => a.LearnerID)
                .Distinct()
                .ToListAsync();

            var newlyAwarded = 0;
            foreach (var learnerId in learnerIds)
            {
                var result = await AchievementAwardService.SyncForLearnerAsync(
                    _context, learnerId);
                newlyAwarded += result.NewAwards;
            }

            return Ok(new
            {
                attemptsStarted,
                submittedAttempts,
                learnersChecked = learnerIds.Count,
                newlyAwarded
            });
        }
    }
}
