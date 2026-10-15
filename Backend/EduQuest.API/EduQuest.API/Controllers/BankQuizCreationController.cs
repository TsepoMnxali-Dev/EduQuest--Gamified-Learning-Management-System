using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers;

// A separate controller avoids replacing existing quiz, attempt, or badge code.
// Only administrators can inspect question-bank availability or create bank-backed quizzes.
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/quizzes")]
public sealed class BankQuizCreationController : ControllerBase
{
    private const int QuestionsPerQuiz = 10;
    private readonly ApplicationDBContext _db;

    public BankQuizCreationController(ApplicationDBContext db)
    {
        _db = db;
    }

    private static readonly string[] DifficultyLevels = { "Easy", "Medium", "Hard" };

    // Same definition for preview and quiz creation. Only copy complete questions:
    // 4 distinct nonempty options, one correct, nonempty text and explanation.
    private async Task<List<QuestionBankItem>> EligibleQuestions(int topicId, string difficulty)
    {
        var candidates = await _db.QuestionBankItems
            .AsNoTracking()
            .Include(q => q.QuestionBankOptions)
            .Where(q => q.TopicID == topicId && q.Difficulty.ToLower() == difficulty.ToLower())
            .ToListAsync();

        return candidates
            .Where(q => !string.IsNullOrWhiteSpace(q.QuestionText)
                     && !string.IsNullOrWhiteSpace(q.Explanation)
                     && q.QuestionBankOptions.Count == 4
                     && q.QuestionBankOptions.Count(o => o.IsCorrect) == 1
                     && q.QuestionBankOptions.All(o => !string.IsNullOrWhiteSpace(o.OptionText))
                     && q.QuestionBankOptions.Select(o => o.OptionText.Trim())
                          .Distinct(StringComparer.OrdinalIgnoreCase).Count() == 4)
            // Avoid repeating the same question when the bank contains duplicate text.
            .GroupBy(q => q.QuestionText.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    [HttpGet("bank-availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] int topicId, [FromQuery] string? difficulty)
    {
        if (topicId <= 0 || string.IsNullOrWhiteSpace(difficulty))
        {
            return BadRequest("Select a topic and difficulty first.");
        }

        var normalized = DifficultyLevels.FirstOrDefault(d =>
            d.Equals(difficulty.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalized is null)
        {
            return BadRequest("Difficulty must be Easy, Medium or Hard.");
        }

        var topic = await _db.Topics.AsNoTracking()
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.TopicID == topicId);
        if (topic is null)
        {
            return NotFound("The topic was not found.");
        }

        var available = (await EligibleQuestions(topicId, normalized)).Count;
        return Ok(new
        {
            topicId = topic.TopicID,
            topicName = topic.TopicName,
            gradeLevel = topic.GradeLevel,
            subjectName = topic.Subject?.SubjectName,
            difficulty = normalized,
            available,
            required = QuestionsPerQuiz,
            canCreate = available >= QuestionsPerQuiz
        });
    }

    // POST /api/quizzes/create-from-bank
    // Accepts the same JSON body as POST /api/quizzes. All 10 questions and
    // the quiz are inserted in one transaction; insufficient items creates nothing.
    [HttpPost("create-from-bank")]
    public async Task<IActionResult> CreateFromBank([FromBody] CreateQuizDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.QuizTitle) || dto.QuizTitle.Trim().Length > 100)
        {
            return BadRequest("Quiz title must be between 1 and 100 characters.");
        }
        if (string.IsNullOrWhiteSpace(dto.Difficulty))
        {
            return BadRequest("Select a difficulty.");
        }

        var difficulty = DifficultyLevels.FirstOrDefault(d =>
            d.Equals(dto.Difficulty.Trim(), StringComparison.OrdinalIgnoreCase));
        if (difficulty is null)
        {
            return BadRequest("Difficulty must be Easy, Medium or Hard.");
        }
        if (dto.TimeLimit <= TimeSpan.Zero || dto.TimeLimit > TimeSpan.FromHours(3))
        {
            return BadRequest("Time limit must be greater than zero and no more than 3 hours.");
        }

        var topic = await _db.Topics.AsNoTracking()
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.TopicID == dto.TopicID);
        if (topic is null)
        {
            return BadRequest("Selected topic does not exist.");
        }

        var title = dto.QuizTitle.Trim();
        if (await _db.Quizzes.AnyAsync(q => q.TopicID == dto.TopicID && q.QuizTitle == title))
        {
            return Conflict("A quiz with this title already exists for this topic.");
        }

        var eligible = await EligibleQuestions(dto.TopicID, difficulty);
        if (eligible.Count < QuestionsPerQuiz)
        {
            return BadRequest(
                $"Not enough valid Question Bank questions for {topic.GradeLevel} / " +
                $"{topic.Subject?.SubjectName} / {topic.TopicName} / {difficulty}. " +
                $"Found {eligible.Count}; {QuestionsPerQuiz} are required. " +
                "Nothing was created. Choose another difficulty, add questions to the bank, " +
                "or create a manual draft.");
        }

        var selected = eligible.OrderBy(_ => Guid.NewGuid()).Take(QuestionsPerQuiz).ToList();
        var quiz = new Quiz
        {
            QuizTitle = title,
            TopicID = topic.TopicID,
            Difficulty = difficulty,
            TimeLimit = dto.TimeLimit,
            IsPublished = dto.IsPublished,
            QuizQuestions = selected.Select(bankItem => new QuizQuestion
            {
                QuestionText = bankItem.QuestionText,
                Explanation = bankItem.Explanation,
                SourceExtract = bankItem.SourceExtract,
                GeneratedByAI = false,
                ApprovedByAdmin = true,
                QuizOptions = bankItem.QuestionBankOptions
                    .OrderBy(o => o.QuestionBankOptionID)
                    .Select(o => new QuizOption
                    {
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
            }).ToList()
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/quizzes/{quiz.QuizID}", new
        {
            quizID = quiz.QuizID,
            quizTitle = quiz.QuizTitle,
            topicID = quiz.TopicID,
            topicName = topic.TopicName,
            gradeLevel = topic.GradeLevel,
            subjectName = topic.Subject?.SubjectName,
            difficulty = quiz.Difficulty,
            timeLimit = quiz.TimeLimit,
            isPublished = quiz.IsPublished,
            questionsAdded = quiz.QuizQuestions.Count,
            source = "Question Bank"
        });
    }
}
