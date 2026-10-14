using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduQuest.API.Controllers
{
    /// <summary>
    /// GET /api/learner/quizzes
    /// The published quizzes a learner can write: the quiz's topic matches the
    /// learner's grade, the learner is enrolled in the subject, and the quiz has
    /// enough approved questions to start (same rule as StartQuiz).
    /// Everything comes from the database.
    /// </summary>
    [Authorize]
    [Route("api/learner")]
    [ApiController]
    public class LearnerQuizzesController : ControllerBase
    {
        // Must match the rule in QuizAttemptsController.StartQuiz.
        private const int QuestionsPerAttempt = 10;

        private readonly ApplicationDBContext _context;

        public LearnerQuizzesController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet("quizzes")]
        public async Task<IActionResult> GetQuizzes()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdText, out var userId))
            {
                return Unauthorized();
            }

            var learner = await _context.Learners
                .Include(l => l.Grade)
                .FirstOrDefaultAsync(l => l.UserID == userId);

            if (learner == null)
            {
                return NotFound("Learner profile not found.");
            }

            var gradeName = learner.Grade?.GradeName;

            var subjectIds = await _context.learnerSubjects
                .Where(ls => ls.LearnerID == learner.LearnerID)
                .Select(ls => ls.SubjectID)
                .ToListAsync();

            // Published quizzes for this learner's grade and subjects.
            var rows = await (
                from q in _context.Quizzes
                join t in _context.Topics on q.TopicID equals t.TopicID
                join s in _context.Subjects on t.SubjectID equals s.SubjectID
                where q.IsPublished
                      && t.GradeLevel == gradeName
                      && subjectIds.Contains(s.SubjectID)
                select new
                {
                    q.QuizID,
                    q.QuizTitle,
                    q.Difficulty,
                    q.TimeLimit,
                    t.TopicName,
                    s.SubjectID,
                    s.SubjectName
                }).ToListAsync();

            var quizIds = rows.Select(r => r.QuizID).ToList();

            // Approved question counts per quiz.
            var approvedCounts = await _context.QuizQuestions
                .Where(q => quizIds.Contains(q.QuizID) && q.ApprovedByAdmin)
                .GroupBy(q => q.QuizID)
                .Select(g => new { QuizID = g.Key, Count = g.Count() })
                .ToListAsync();

            var readyQuizIds = approvedCounts
                .Where(c => c.Count >= QuestionsPerAttempt)
                .Select(c => c.QuizID)
                .ToHashSet();

            // Submitted attempts (an attempt is submitted when it has answers).
            var submitted = await _context.QuizAttempts
                .Where(a => a.LearnerID == learner.LearnerID
                            && quizIds.Contains(a.QuizID)
                            && a.QuizAttemptAnswers.Any())
                .Select(a => new
                {
                    a.QuizAttemptID,
                    a.QuizID,
                    a.Score,
                    a.DateTaken,
                    Total = a.QuizAttemptQuestions.Count()
                })
                .ToListAsync();

            var latestByQuiz = submitted
                .GroupBy(a => a.QuizID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.DateTaken)
                          .ThenByDescending(a => a.QuizAttemptID)
                          .First());

            var subjects = rows
                .Where(r => readyQuizIds.Contains(r.QuizID))
                .GroupBy(r => new { r.SubjectID, r.SubjectName })
                .OrderBy(g => g.Key.SubjectName)
                .Select(g => new
                {
                    subjectID = g.Key.SubjectID,
                    subjectName = g.Key.SubjectName,
                    quizzes = g.OrderBy(r => r.QuizTitle).Select(r =>
                    {
                        latestByQuiz.TryGetValue(r.QuizID, out var latest);

                        int? percent = latest != null && latest.Total > 0
                            ? (int)Math.Round(latest.Score * 100.0 / latest.Total)
                            : (int?)null;

                        return new
                        {
                            quizID = r.QuizID,
                            title = r.QuizTitle,
                            topicName = r.TopicName,
                            difficulty = r.Difficulty,
                            timeLimitMinutes = (int)Math.Round(r.TimeLimit.TotalMinutes),
                            questionCount = QuestionsPerAttempt,
                            completed = latest != null,
                            latestAttemptID = latest?.QuizAttemptID,
                            latestScorePercent = percent,
                            latestDate = latest?.DateTaken
                        };
                    }).ToList()
                })
                .ToList();

            var allQuizzes = subjects.SelectMany(s => s.quizzes).ToList();
            var completedQuizzes = allQuizzes.Where(q => q.completed).ToList();

            double? average = completedQuizzes.Count > 0
                ? Math.Round(completedQuizzes.Average(q => q.latestScorePercent ?? 0))
                : (double?)null;

            return Ok(new
            {
                subjects,
                summary = new
                {
                    completed = completedQuizzes.Count,
                    todo = allQuizzes.Count - completedQuizzes.Count,
                    averagePercent = average
                }
            });
        }
    }
}