using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/quizattempts")]
    [ApiController]
    public class QuizAttemptsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public QuizAttemptsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/quizattempts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttemptDto>>> GetQuizAttempts()
        {
            var attempts = await _context.QuizAttempts
                .Select(attempt => new QuizAttemptDto
                {
                    QuizAttemptID = attempt.QuizAttemptID,
                    Score = attempt.Score,
                    DateTaken = attempt.DateTaken,
                    TimeTaken = attempt.TimeTaken,
                    QuizID = attempt.QuizID,
                    LearnerID = attempt.LearnerID
                })
                .ToListAsync();

            return Ok(attempts);
        }

        // GET: api/quizattempts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAttemptDto>> GetQuizAttempt(int id)
        {
            var attempt = await _context.QuizAttempts
                .Where(attempt => attempt.QuizAttemptID == id)
                .Select(attempt => new QuizAttemptDto
                {
                    QuizAttemptID = attempt.QuizAttemptID,
                    Score = attempt.Score,
                    DateTaken = attempt.DateTaken,
                    TimeTaken = attempt.TimeTaken,
                    QuizID = attempt.QuizID,
                    LearnerID = attempt.LearnerID
                })
                .FirstOrDefaultAsync();

            if (attempt == null)
            {
                return NotFound();
            }

            return Ok(attempt);
        }

        // POST: api/quizattempts
        // No [Authorize(Roles = "Admin")] here — any authenticated learner
        // needs to be able to submit their own attempt.
        [HttpPost]
        public async Task<ActionResult<QuizAttemptDto>> CreateQuizAttempt(CreateQuizAttemptDto dto)
        {
            // Layer 2: Check that the referenced Quiz actually exists
            var quizExists = await _context.Quizzes
                .AnyAsync(quiz => quiz.QuizID == dto.QuizID);

            if (!quizExists)
            {
                return BadRequest("The specified QuizID does not exist.");
            }

            // Layer 2: Check that the referenced Learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == dto.LearnerID);

            if (!learnerExists)
            {
                return BadRequest("The specified LearnerID does not exist.");
            }

            // NOTE: Score is trusted as-is from the client here.
            // In a production system, Score would typically be computed
            // server-side from the learner's submitted QuizAttemptAnswers,
            // rather than accepted directly — otherwise a client can submit
            // any score it wants.

            var attemptEntity = new QuizAttempt
            {
                Score = dto.Score,
                DateTaken = dto.DateTaken.Trim(),
                TimeTaken = dto.TimeTaken.Trim(),
                QuizID = dto.QuizID,
                LearnerID = dto.LearnerID
            };

            _context.QuizAttempts.Add(attemptEntity);
            await _context.SaveChangesAsync();

            var result = new QuizAttemptDto
            {
                QuizAttemptID = attemptEntity.QuizAttemptID,
                Score = attemptEntity.Score,
                DateTaken = attemptEntity.DateTaken,
                TimeTaken = attemptEntity.TimeTaken,
                QuizID = attemptEntity.QuizID,
                LearnerID = attemptEntity.LearnerID
            };

            return CreatedAtAction(
                nameof(GetQuizAttempt),
                new { id = attemptEntity.QuizAttemptID },
                result
            );
        }

        // GET: api/learners/{learnerId}/quizattempts
        // Absolute override: this does NOT sit under api/quizattempts
        [HttpGet("/api/learners/{learnerId}/quizattempts")]
        public async Task<ActionResult<IEnumerable<QuizAttemptDto>>> GetQuizAttemptsForLearner(int learnerId)
        {
            // Layer 2: Make sure the learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == learnerId);

            if (!learnerExists)
            {
                return NotFound("The specified learner does not exist.");
            }

            var attempts = await _context.QuizAttempts
                .Where(attempt => attempt.LearnerID == learnerId)
                .Select(attempt => new QuizAttemptDto
                {
                    QuizAttemptID = attempt.QuizAttemptID,
                    Score = attempt.Score,
                    DateTaken = attempt.DateTaken,
                    TimeTaken = attempt.TimeTaken,
                    QuizID = attempt.QuizID,
                    LearnerID = attempt.LearnerID
                })
                .ToListAsync();

            return Ok(attempts);
        }
    }
}