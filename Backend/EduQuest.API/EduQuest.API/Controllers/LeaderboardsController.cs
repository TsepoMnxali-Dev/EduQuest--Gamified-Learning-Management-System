
using EduQuest.API.Data;
using EduQuest.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // ==============================================
        // GET ALL STORED LEADERBOARD RECORDS
        // ==============================================

        // GET: api/leaderboards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaderboardDto>>>
            GetLeaderboards()
        {
            var leaderboards = await _context.LeaderBoards
                .AsNoTracking()
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

        // ==============================================
        // LIVE LEADERBOARD RANKINGS
        // ==============================================

        // GET: api/leaderboards/rankings
        //
        // Supports:
        // period = week, month, all
        // subjectId, provinceId, gradeId
        //
        // CHANGED:
        // Only learners who earned points appear.
        // Learners with 0 points are excluded.
        //
        // Points are calculated from submitted
        // quiz attempts, not dummy data.
        // ==============================================

        [HttpGet("rankings")]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>>
            GetRankings(
                string period = "all",
                int? subjectId = null,
                int? provinceId = null,
                int? gradeId = null)
        {
            // NEW: Validate the period.
            period = period?.Trim().ToLowerInvariant() ?? "all";

            if (period != "week" &&
                period != "month" &&
                period != "all")
            {
                return BadRequest(
                    "Period must be week, month or all.");
            }

            // ==========================================
            // STEP 1: GET QUIZ ATTEMPTS
            // ==========================================

            var query = _context.QuizAttempts
                .AsNoTracking()
                .AsQueryable();

            // Only attempts with saved answers
            // are considered submitted.
            query = query.Where(a =>
                a.QuizAttemptAnswers.Any());

            // ==========================================
            // STEP 2: FILTER BY TIME PERIOD
            // ==========================================

            if (period == "week")
            {
                var since = DateTime.UtcNow.AddDays(-7);

                query = query.Where(a =>
                    a.DateTaken >= since);
            }
            else if (period == "month")
            {
                var since = DateTime.UtcNow.AddDays(-30);

                query = query.Where(a =>
                    a.DateTaken >= since);
            }

            // ==========================================
            // STEP 3: OPTIONAL FILTERS
            // ==========================================

            if (subjectId.HasValue)
            {
                query = query.Where(a =>
                    a.Quiz!.Topic!.SubjectID ==
                    subjectId.Value);
            }

            if (provinceId.HasValue)
            {
                query = query.Where(a =>
                    a.Learner!.School.ProvinceID ==
                    provinceId.Value);
            }

            if (gradeId.HasValue)
            {
                query = query.Where(a =>
                    a.Learner!.GradeID ==
                    gradeId.Value);
            }

            // ==========================================
            // STEP 4: GET ATTEMPTS FROM DATABASE
            // ==========================================

            var attempts = await query
                .Select(a => new
                {
                    a.LearnerID,

                    FirstName =
                        a.Learner!.User!.FirstName,

                    LastName =
                        a.Learner.User.LastName,

                    UserID =
                        a.Learner.UserID,

                    SchoolName =
                        a.Learner.School.SchoolName,

                    ProvinceID =
                        a.Learner.School.ProvinceID,

                    ProvinceName =
                        a.Learner.School.Province.ProvinceName,

                    GradeName =
                        a.Learner.Grade.GradeName,

                    a.Score,

                    Total =
                        a.QuizAttemptQuestions.Count
                })
                .ToListAsync();

            // NEW:
            // If nobody has submitted a quiz,
            // return an empty leaderboard.
            if (attempts.Count == 0)
            {
                return Ok(
                    new List<LeaderboardEntryDto>());
            }

            // ==========================================
            // STEP 5: USER INFORMATION
            // ==========================================

            bool isAdmin = User.IsInRole("Admin");

            string? currentUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // Preserve existing name privacy:
            // Admins see full names.
            // Learners see first name and surname initial.
            string Display(string first, string last)
            {
                if (isAdmin ||
                    string.IsNullOrWhiteSpace(last))
                {
                    return $"{first} {last}".Trim();
                }

                return $"{first} {last[0]}.";
            }

            // ==========================================
            // STEP 6: CALCULATE LEARNER POINTS
            // ==========================================

            var entries = attempts
                .GroupBy(a => a.LearnerID)
                .Select(group =>
                {
                    var first = group.First();

                    var withQuestions = group
                        .Where(a => a.Total > 0)
                        .ToList();

                    // Points = total correct answers
                    // from submitted quiz attempts.
                    int totalPoints = group.Sum(a =>
                        a.Score);

                    double averagePercent =
                        withQuestions.Count == 0
                        ? 0
                        : Math.Round(
                            withQuestions.Average(a =>
                                a.Score * 100.0 /
                                a.Total),
                            1);

                    return new LeaderboardEntryDto
                    {
                        LearnerID = group.Key,

                        DisplayName = Display(
                            first.FirstName,
                            first.LastName),

                        SchoolName = first.SchoolName,

                        ProvinceID = first.ProvinceID,

                        ProvinceName = first.ProvinceName,

                        GradeName = first.GradeName,

                        Points = totalPoints,

                        QuizzesTaken = group.Count(),

                        AveragePercent = averagePercent,

                        IsCurrentUser =
                            currentUserId != null &&
                            first.UserID.ToString() ==
                            currentUserId
                    };
                })

                // ======================================
                // CHANGED: EXCLUDE ZERO-POINT LEARNERS
                // ======================================

                // Learners must earn at least 1 point
                // to qualify for the leaderboard.
                .Where(entry => entry.Points > 0)

                // Highest points first.
                .OrderByDescending(entry => entry.Points)

                // CHANGED:
                // Alphabetical tiebreaker when points
                // are equal.
                .ThenBy(
                    entry => entry.DisplayName,
                    StringComparer.OrdinalIgnoreCase)

                // Final consistent tiebreaker.
                .ThenBy(entry => entry.LearnerID)

                .ToList();

            // ==========================================
            // STEP 7: ASSIGN RANKINGS
            // ==========================================

            // CHANGED:
            // Every learner gets a sequential position.
            // Equal points are ordered alphabetically.

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            // If everyone scored zero,
            // entries is empty and returns [].
            return Ok(entries);
        }

        // ==============================================
        // GET ONE STORED LEADERBOARD RECORD
        // ==============================================

        // GET: api/leaderboards/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaderboardDto>>
            GetLeaderboard(int id)
        {
            var leaderboard = await _context.LeaderBoards
                .AsNoTracking()
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
                return NotFound(
                    "Leaderboard record not found.");
            }

            return Ok(leaderboard);
        }

        // ==============================================
        // GET STORED LEADERBOARDS BY GRADE
        // ==============================================

        // GET: api/leaderboards/grade/{gradeId}
        [HttpGet("grade/{gradeId}")]
        public async Task<ActionResult<IEnumerable<LeaderboardDto>>>
            GetLeaderboardsByGrade(int gradeId)
        {
            var gradeExists = await _context.Grades
                .AnyAsync(grade =>
                    grade.GradeID == gradeId);

            if (!gradeExists)
            {
                return NotFound(
                    "The specified grade does not exist.");
            }

            var leaderboards = await _context.LeaderBoards
                .AsNoTracking()
                .Where(lb =>
                    lb.Learner != null &&
                    lb.Learner.GradeID == gradeId)
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
