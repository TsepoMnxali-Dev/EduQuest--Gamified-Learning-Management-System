using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers;

/// <summary>
/// The single source of truth for quiz progress and review links.
/// This uses EXACTLY the same submitted-answer test as StartQuiz in
/// QuizAttemptsController: an attempt is submitted when a saved answer exists.
/// The response is scoped to the authenticated learner.
/// </summary>
[ApiController]
[Authorize(Roles = "Learner")]
[Route("api/learner-quiz-progress")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class LearnerQuizProgressController : ControllerBase
{
    private const int MaxAttempts = 2;
    private readonly ApplicationDBContext _db;

    public LearnerQuizProgressController(ApplicationDBContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProgress()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        int? learnerId = await _db.Learners.AsNoTracking()
            .Where(learner => learner.UserID == userId)
            .Select(learner => (int?)learner.LearnerID)
            .FirstOrDefaultAsync();

        if (!learnerId.HasValue)
            return NotFound("Your account does not have a learner profile.");

        // IMPORTANT: use the SAME predicate as StartQuiz and SubmitQuizAttempt.
        // In particular, do not depend on populated navigation collections or
        // require answers.Count == questions.Count in the progress query.
        var rows = await _db.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.LearnerID == learnerId.Value)
            .Select(attempt => new
            {
                attempt.QuizAttemptID,
                attempt.QuizID,
                attempt.Score,
                attempt.DateTaken,
                QuestionCount = _db.QuizAttemptQuestions.Count(question =>
                    question.QuizAttemptID == attempt.QuizAttemptID),
                HasSubmittedAnswers = _db.QuizAttemptAnswers.Any(answer =>
                    answer.AttemptID == attempt.QuizAttemptID)
            })
            .ToListAsync();

        var progress = rows
            .Where(attempt => attempt.HasSubmittedAnswers)
            .GroupBy(attempt => attempt.QuizID)
            .Select(group =>
            {
                var ordered = group
                    .OrderBy(attempt => attempt.DateTaken)
                    .ThenBy(attempt => attempt.QuizAttemptID)
                    .ToList();
                var best = ordered
                    .OrderByDescending(attempt => attempt.QuestionCount > 0
                        ? (double)attempt.Score / attempt.QuestionCount
                        : 0)
                    .ThenByDescending(attempt => attempt.QuizAttemptID)
                    .First();
                var latest = ordered.Last();

                var history = ordered.Select(attempt => new
                {
                    attemptID = attempt.QuizAttemptID,
                    score = attempt.Score,
                    totalQuestions = attempt.QuestionCount,
                    percentage = attempt.QuestionCount > 0
                        ? Math.Round(100.0 * attempt.Score / attempt.QuestionCount, 2)
                        : 0,
                    dateTaken = attempt.DateTaken
                }).ToList();

                return new
                {
                    quizID = group.Key,
                    quizAttemptID = best.QuizAttemptID,
                    bestAttemptID = best.QuizAttemptID,
                    latestAttemptID = latest.QuizAttemptID,
                    score = best.Score,
                    totalQuestions = best.QuestionCount,
                    percentage = best.QuestionCount > 0
                        ? Math.Round(100.0 * best.Score / best.QuestionCount, 2)
                        : 0,
                    dateTaken = latest.DateTaken,
                    completedAttempts = ordered.Count,
                    maxAttempts = MaxAttempts,
                    remainingAttempts = Math.Max(0, MaxAttempts - ordered.Count),
                    attempts = history
                };
            })
            .OrderBy(record => record.quizID)
            .ToList();

        return Ok(progress);
    }
}
