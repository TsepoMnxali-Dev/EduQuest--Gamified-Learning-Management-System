using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    // Public numbers for the landing page. Contains counts only,
    // no personal information, so it is safe to leave anonymous.
    [AllowAnonymous]
    [ApiController]
    [Route("api/landing-stats")]
    public class LandingStatsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LandingStatsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/landing-stats
        [HttpGet]
        public async Task<IActionResult> GetLandingStats()
        {
            var studentsEnrolled = await _context.Learners.CountAsync();

            var schoolsReached = await _context.Learners
                .Select(l => l.SchoolID)
                .Distinct()
                .CountAsync();

            // Quizzes completed, counted the same way as the learner Quizzes page:
            //  - an attempt with saved answers has been submitted;
            //  - a quiz counts once per learner, even after a retake;
            //  - only published quizzes count.
            var quizzesCompleted = await _context.QuizAttempts
                .Where(a => _context.QuizAttemptAnswers
                    .Any(ans => ans.AttemptID == a.QuizAttemptID))
                .Where(a => _context.Quizzes
                    .Any(q => q.QuizID == a.QuizID && q.IsPublished))
                .Select(a => new { a.LearnerID, a.QuizID })
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                studentsEnrolled,
                schoolsReached,
                quizzesCompleted
            });
        }
    }
}