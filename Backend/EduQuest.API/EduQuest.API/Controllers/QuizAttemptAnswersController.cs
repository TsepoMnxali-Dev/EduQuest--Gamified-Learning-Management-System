using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/quizattemptanswers")]
    [ApiController]
    public class QuizAttemptAnswersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public QuizAttemptAnswersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST: api/quizattemptanswers
        // No [Authorize(Roles = "Admin")] — a learner submits their own answers.
        [HttpPost]
        public async Task<ActionResult<QuizAttemptAnswerDto>> CreateAnswer(CreateQuizAttemptAnswerDto dto)
        {
            // Layer 2: Check that the referenced Attempt actually exists
            var attempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.QuizAttemptID == dto.AttemptID);

            if (attempt == null)
            {
                return BadRequest("The specified AttemptID does not exist.");
            }

            // Layer 2: Check that the referenced Question actually exists
            var question = await _context.QuizQuestions
                .FirstOrDefaultAsync(q => q.QuizQuestionID == dto.QuestionID);

            if (question == null)
            {
                return BadRequest("The specified QuestionID does not exist.");
            }

            // Layer 2: Check that the Question actually belongs to the Attempt's Quiz
            if (question.QuizID != attempt.QuizID)
            {
                return BadRequest("This question does not belong to the quiz for this attempt.");
            }

            // Layer 2: Check that the referenced Option actually exists
            var option = await _context.QuizOptions
                .FirstOrDefaultAsync(o => o.QuizOptionID == dto.QuizOptionID);

            if (option == null)
            {
                return BadRequest("The specified QuizOptionID does not exist.");
            }

            // Layer 2: Check that the Option actually belongs to the Question
            if (option.QuestionID != dto.QuestionID)
            {
                return BadRequest("This option does not belong to the specified question.");
            }

            // Layer 2: Prevent answering the same question twice in one attempt
            var alreadyAnswered = await _context.QuizAttemptAnswers
                .AnyAsync(a =>
                    a.AttemptID == dto.AttemptID &&
                    a.QuestionID == dto.QuestionID);

            if (alreadyAnswered)
            {
                return Conflict("This question has already been answered for this attempt.");
            }

            var answerEntity = new QuizAttemptAnswer
            {
                AttemptID = dto.AttemptID,
                QuestionID = dto.QuestionID,
                QuizOptionID = dto.QuizOptionID
            };

            _context.QuizAttemptAnswers.Add(answerEntity);
            await _context.SaveChangesAsync();

            // NOTE: This does not update QuizAttempt.Score.
            // If scoring should be derived from answers, this is where
            // you'd recalculate and save the attempt's Score — e.g.:
            //
            // if (option.IsCorrect)
            // {
            //     attempt.Score += 1;
            //     await _context.SaveChangesAsync();
            // }

            var result = new QuizAttemptAnswerDto
            {
                QuizAttemptAnswerID = answerEntity.QuizAttemptAnswerID,
                AttemptID = answerEntity.AttemptID,
                QuestionID = answerEntity.QuestionID,
                QuizOptionID = answerEntity.QuizOptionID
            };

            // No single-item GET endpoint exists for answers,
            // so we point the Location header at the list endpoint instead.
            return CreatedAtAction(
                nameof(GetAnswersForAttempt),
                new { attemptId = answerEntity.AttemptID },
                result
            );
        }

        // GET: api/quizattempts/{attemptId}/answers
        // Absolute override: this does NOT sit under api/quizattemptanswers
        [HttpGet("/api/quizattempts/{attemptId}/answers")]
        public async Task<ActionResult<IEnumerable<QuizAttemptAnswerDto>>> GetAnswersForAttempt(int attemptId)
        {
            // Layer 2: Make sure the attempt actually exists
            var attemptExists = await _context.QuizAttempts
                .AnyAsync(a => a.QuizAttemptID == attemptId);

            if (!attemptExists)
            {
                return NotFound("The specified attempt does not exist.");
            }

            var answers = await _context.QuizAttemptAnswers
                .Where(a => a.AttemptID == attemptId)
                .Select(a => new QuizAttemptAnswerDto
                {
                    QuizAttemptAnswerID = a.QuizAttemptAnswerID,
                    AttemptID = a.AttemptID,
                    QuestionID = a.QuestionID,
                    QuizOptionID = a.QuizOptionID
                })
                .ToListAsync();

            return Ok(answers);
        }
    }
}