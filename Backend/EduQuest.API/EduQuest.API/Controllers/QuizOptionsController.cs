using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/questions/{questionId}/options")]
    [ApiController]
    public class QuizOptionsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public QuizOptionsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/questions/{questionId}/options
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizOptionDto>>> GetOptions(int questionId)
        {
            // Layer 2: Make sure the parent question actually exists
            var questionExists = await _context.QuizQuestions
                .AnyAsync(question => question.QuizQuestionID == questionId);

            if (!questionExists)
            {
                return NotFound("The specified question does not exist.");
            }

            var options = await _context.QuizOptions
                .Where(option => option.QuestionID == questionId)
                .Select(option => new QuizOptionDto
                {
                    QuizOptionID = option.QuizOptionID,
                    OptionText = option.OptionText,
                    IsCorrect = option.IsCorrect,
                    QuestionID = option.QuestionID
                })
                .ToListAsync();

            return Ok(options);
        }


        [Authorize(Roles = "Admin")]

        // POST: api/questions/{questionId}/options
        [HttpPost]
        public async Task<ActionResult<QuizOptionDto>> CreateOption(
            int questionId,
            CreateQuizOptionDto dto)
        {
            // Layer 2: Make sure the parent question actually exists
            var questionExists = await _context.QuizQuestions
                .AnyAsync(question => question.QuizQuestionID == questionId);

            if (!questionExists)
            {
                return NotFound("The specified question does not exist.");
            }

            // Normalize the input
            var optionText = dto.OptionText.Trim();

            // Layer 2: Prevent duplicate option text within the same question
            var exists = await _context.QuizOptions
                .AnyAsync(option =>
                    option.QuestionID == questionId &&
                    option.OptionText == optionText);

            if (exists)
            {
                return Conflict("This option already exists for this question.");
            }

            var optionEntity = new QuizOption
            {
                OptionText = optionText,
                IsCorrect = dto.IsCorrect,
                QuestionID = questionId
            };

            _context.QuizOptions.Add(optionEntity);
            await _context.SaveChangesAsync();

            var result = new QuizOptionDto
            {
                QuizOptionID = optionEntity.QuizOptionID,
                OptionText = optionEntity.OptionText,
                IsCorrect = optionEntity.IsCorrect,
                QuestionID = optionEntity.QuestionID
            };

            // No single-item GET endpoint exists for options,
            // so we point the Location header at the list endpoint instead.
            return CreatedAtAction(
                nameof(GetOptions),
                new { questionId = optionEntity.QuestionID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        // PUT: api/options/{id}
        // Absolute override: this does NOT sit under api/questions/{questionId}/options
        [HttpPut("/api/options/{id}")]
        public async Task<IActionResult> UpdateOption(
            int id,
            UpdateQuizOptionDto dto)
        {
            var optionEntity = await _context.QuizOptions.FindAsync(id);

            if (optionEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var optionText = dto.OptionText.Trim();

            // Layer 2: Prevent duplicate option text within the same question
            // Exclude the current option from the check.
            var exists = await _context.QuizOptions
                .AnyAsync(option =>
                    option.QuestionID == optionEntity.QuestionID &&
                    option.OptionText == optionText &&
                    option.QuizOptionID != id);

            if (exists)
            {
                return Conflict("This option already exists for this question.");
            }

            optionEntity.OptionText = optionText;
            optionEntity.IsCorrect = dto.IsCorrect;
            // QuestionID intentionally left unchanged — see UpdateQuizOptionDto note above

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        // DELETE: api/options/{id}
        // Absolute override: this does NOT sit under api/questions/{questionId}/options
        [HttpDelete("/api/options/{id}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var optionEntity = await _context.QuizOptions.FindAsync(id);

            if (optionEntity == null)
            {
                return NotFound();
            }

            _context.QuizOptions.Remove(optionEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}