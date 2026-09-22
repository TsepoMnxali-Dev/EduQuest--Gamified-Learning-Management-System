using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        // Layer 2: Business rule
        // EduQuest only supports these difficulty levels for the current version.
        private static readonly HashSet<string> AllowedDifficulties =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Easy",
                "Medium",
                "Hard"
            };

        // Layer 2: Business rule
        // Quiz time limits must be reasonable — greater than zero, capped at 3 hours.
        private static readonly TimeSpan MinTimeLimit = TimeSpan.Zero;
        private static readonly TimeSpan MaxTimeLimit = TimeSpan.FromHours(3);

        public QuizzesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/quizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzes()
        {
            var quizzes = await _context.Quizzes
                .Select(quiz => new QuizDto
                {
                    QuizID = quiz.QuizID,
                    QuizTitle = quiz.QuizTitle,
                    Difficulty = quiz.Difficulty,
                    TimeLimit = quiz.TimeLimit,
                    IsPublished = quiz.IsPublished,
                    TopicID = quiz.TopicID
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        // GET: api/quizzes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDto>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Where(quiz => quiz.QuizID == id)
                .Select(quiz => new QuizDto
                {
                    QuizID = quiz.QuizID,
                    QuizTitle = quiz.QuizTitle,
                    Difficulty = quiz.Difficulty,
                    TimeLimit = quiz.TimeLimit,
                    IsPublished = quiz.IsPublished,
                    TopicID = quiz.TopicID
                })
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return NotFound();
            }

            return Ok(quiz);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<QuizDto>> CreateQuiz(CreateQuizDto dto)
        {
            // Normalize the input
            var quizTitle = dto.QuizTitle.Trim();
            var difficulty = dto.Difficulty.Trim();

            // Layer 2: Check that the difficulty is Easy, Medium, or Hard
            var validDifficulty = AllowedDifficulties.FirstOrDefault(
                d => string.Equals(
                    d,
                    difficulty,
                    StringComparison.OrdinalIgnoreCase));

            if (validDifficulty == null)
            {
                return BadRequest(
                    "Difficulty must be Easy, Medium, or Hard.");
            }

            // Store the canonical version
            difficulty = validDifficulty;

            // Layer 2: TimeLimit must be a positive duration, capped at 3 hours
            if (dto.TimeLimit <= MinTimeLimit)
            {
                return BadRequest("TimeLimit must be greater than zero.");
            }

            if (dto.TimeLimit > MaxTimeLimit)
            {
                return BadRequest("TimeLimit cannot exceed 3 hours.");
            }

            // Layer 2: Check that the referenced Topic actually exists
            var topicExists = await _context.Topics
                .AnyAsync(topic => topic.TopicID == dto.TopicID);

            if (!topicExists)
            {
                return BadRequest("The specified TopicID does not exist.");
            }

            // Layer 2: Prevent duplicate quiz titles within the same topic
            var exists = await _context.Quizzes
                .AnyAsync(quiz =>
                    quiz.QuizTitle == quizTitle &&
                    quiz.TopicID == dto.TopicID);

            if (exists)
            {
                return Conflict("A quiz with this title already exists for this topic.");
            }

            var quizEntity = new Quiz
            {
                QuizTitle = quizTitle,
                Difficulty = difficulty,
                TimeLimit = dto.TimeLimit,
                IsPublished = dto.IsPublished,
                TopicID = dto.TopicID
            };

            _context.Quizzes.Add(quizEntity);
            await _context.SaveChangesAsync();

            var result = new QuizDto
            {
                QuizID = quizEntity.QuizID,
                QuizTitle = quizEntity.QuizTitle,
                Difficulty = quizEntity.Difficulty,
                TimeLimit = quizEntity.TimeLimit,
                IsPublished = quizEntity.IsPublished,
                TopicID = quizEntity.TopicID
            };

            return CreatedAtAction(
                nameof(GetQuiz),
                new { id = quizEntity.QuizID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(
            int id,
            UpdateQuizDto dto)
        {
            var quizEntity = await _context.Quizzes.FindAsync(id);

            if (quizEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var quizTitle = dto.QuizTitle.Trim();
            var difficulty = dto.Difficulty.Trim();

            // Layer 2: Check that the difficulty is Easy, Medium, or Hard
            var validDifficulty = AllowedDifficulties.FirstOrDefault(
                d => string.Equals(
                    d,
                    difficulty,
                    StringComparison.OrdinalIgnoreCase));

            if (validDifficulty == null)
            {
                return BadRequest(
                    "Difficulty must be Easy, Medium, or Hard.");
            }

            difficulty = validDifficulty;

            // Layer 2: TimeLimit must be a positive duration, capped at 3 hours
            if (dto.TimeLimit <= MinTimeLimit)
            {
                return BadRequest("TimeLimit must be greater than zero.");
            }

            if (dto.TimeLimit > MaxTimeLimit)
            {
                return BadRequest("TimeLimit cannot exceed 3 hours.");
            }

            // Layer 2: Check that the referenced Topic actually exists
            var topicExists = await _context.Topics
                .AnyAsync(topic => topic.TopicID == dto.TopicID);

            if (!topicExists)
            {
                return BadRequest("The specified TopicID does not exist.");
            }

            // Layer 2: Prevent duplicate quiz titles within the same topic
            // Exclude the current quiz from the check.
            var exists = await _context.Quizzes
                .AnyAsync(quiz =>
                    quiz.QuizTitle == quizTitle &&
                    quiz.TopicID == dto.TopicID &&
                    quiz.QuizID != id);

            if (exists)
            {
                return Conflict("A quiz with this title already exists for this topic.");
            }

            quizEntity.QuizTitle = quizTitle;
            quizEntity.Difficulty = difficulty;
            quizEntity.TimeLimit = dto.TimeLimit;
            quizEntity.IsPublished = dto.IsPublished;
            quizEntity.TopicID = dto.TopicID;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quizEntity = await _context.Quizzes.FindAsync(id);

            if (quizEntity == null)
            {
                return NotFound();
            }

            _context.Quizzes.Remove(quizEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}