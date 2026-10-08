using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/questionbank")]
    [ApiController]
    public class QuestionBankController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public QuestionBankController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/questionbank?topicId=71&difficulty=Easy
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionBankItemDto>>> GetBankItems(
            [FromQuery] int? topicId,
            [FromQuery] string? difficulty)
        {
            var query = _context.QuestionBankItems
                .Include(qbi => qbi.Topic)
                .Include(qbi => qbi.QuestionBankOptions)
                .AsQueryable();

            if (topicId.HasValue)
            {
                query = query.Where(qbi => qbi.TopicID == topicId.Value);
            }

            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                query = query.Where(qbi => qbi.Difficulty == difficulty);
            }

            var items = await query
                .Select(qbi => new QuestionBankItemDto
                {
                    QuestionBankItemID = qbi.QuestionBankItemID,
                    TopicID = qbi.TopicID,
                    TopicName = qbi.Topic!.TopicName,
                    QuestionText = qbi.QuestionText,
                    SourceExtract = qbi.SourceExtract,
                    Explanation = qbi.Explanation,
                    Difficulty = qbi.Difficulty,
                    Source = qbi.Source,
                    Options = qbi.QuestionBankOptions
                        .Select(o => new QuestionBankOptionDto
                        {
                            QuestionBankOptionID = o.QuestionBankOptionID,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/questionbank/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionBankItemDto>> GetBankItem(int id)
        {
            var item = await _context.QuestionBankItems
                .Include(qbi => qbi.Topic)
                .Include(qbi => qbi.QuestionBankOptions)
                .Where(qbi => qbi.QuestionBankItemID == id)
                .Select(qbi => new QuestionBankItemDto
                {
                    QuestionBankItemID = qbi.QuestionBankItemID,
                    TopicID = qbi.TopicID,
                    TopicName = qbi.Topic!.TopicName,
                    QuestionText = qbi.QuestionText,
                    SourceExtract = qbi.SourceExtract,
                    Explanation = qbi.Explanation,
                    Difficulty = qbi.Difficulty,
                    Source = qbi.Source,
                    Options = qbi.QuestionBankOptions
                        .Select(o => new QuestionBankOptionDto
                        {
                            QuestionBankOptionID = o.QuestionBankOptionID,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<QuestionBankItemDto>> UpdateBankItem(
    int id,
    [FromBody] UpdateQuestionBankItemDto dto)
        {
            // Find the existing question
            var questionBankItem = await _context.QuestionBankItems
                .Include(qbi => qbi.QuestionBankOptions)
                .FirstOrDefaultAsync(qbi => qbi.QuestionBankItemID == id);

            if (questionBankItem == null)
            {
                return NotFound();
            }

            // Check that the topic exists
            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicID == dto.TopicID);

            if (topic == null)
            {
                return BadRequest("The specified TopicID does not exist.");
            }

            // Check that the difficulty is valid
            var validDifficulties = new[] { "Easy", "Medium", "Hard" };

            if (!validDifficulties.Contains(dto.Difficulty))
            {
                return BadRequest("Difficulty must be Easy, Medium, or Hard.");
            }

            // A Question Bank question must have exactly 4 options
            if (dto.Options.Count != 4)
            {
                return BadRequest("A question must have exactly 4 options.");
            }

            // A Question Bank question must have exactly one correct answer
            if (dto.Options.Count(o => o.IsCorrect) != 1)
            {
                return BadRequest("A question must have exactly one correct option.");
            }

            // Make sure none of the options are empty
            if (dto.Options.Any(o => string.IsNullOrWhiteSpace(o.OptionText)))
            {
                return BadRequest("Options cannot be empty.");
            }

            // Make sure options are unique
            if (dto.Options
                .GroupBy(o => o.OptionText.Trim().ToLower())
                .Any(g => g.Count() > 1))
            {
                return BadRequest("Options must be unique.");
            }

            // Update the question
            questionBankItem.TopicID = dto.TopicID;
            questionBankItem.QuestionText = dto.QuestionText;
            questionBankItem.SourceExtract = dto.SourceExtract;
            questionBankItem.Explanation = dto.Explanation;
            questionBankItem.Difficulty = dto.Difficulty;

            // Remove the old options
            _context.QuestionBankOptions.RemoveRange(
                questionBankItem.QuestionBankOptions);

            // Add the new options
            foreach (var optionDto in dto.Options)
            {
                questionBankItem.QuestionBankOptions.Add(
                    new QuestionBankOption
                    {
                        OptionText = optionDto.OptionText,
                        IsCorrect = optionDto.IsCorrect
                    });
            }

            await _context.SaveChangesAsync();

            // Return the updated question
            var result = new QuestionBankItemDto
            {
                QuestionBankItemID = questionBankItem.QuestionBankItemID,
                TopicID = questionBankItem.TopicID,
                TopicName = topic.TopicName,
                QuestionText = questionBankItem.QuestionText,
                SourceExtract = questionBankItem.SourceExtract,
                Explanation = questionBankItem.Explanation,
                Difficulty = questionBankItem.Difficulty,
                Source = questionBankItem.Source,
                Options = questionBankItem.QuestionBankOptions
                    .Select(o => new QuestionBankOptionDto
                    {
                        QuestionBankOptionID = o.QuestionBankOptionID,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<QuestionBankItemDto>> CreateBankItem(
    [FromBody] CreateQuestionBankItemDto dto)
        {
            // Check that the topic exists
            var topic = await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicID == dto.TopicID);

            if (topic == null)
            {
                return BadRequest("The specified TopicID does not exist.");
            }
            var validDifficulties = new[] { "Easy", "Medium", "Hard" };

            if (!validDifficulties.Contains(dto.Difficulty))
            {
                return BadRequest("Difficulty must be Easy, Medium, or Hard.");
            }

            // A Question Bank question must have exactly 4 options
            if (dto.Options.Count != 4)
            {
                return BadRequest("A question must have exactly 4 options.");
            }

            // A Question Bank question must have exactly one correct answer
            if (dto.Options.Count(o => o.IsCorrect) != 1)
            {
                return BadRequest("A question must have exactly one correct option.");
            }

            // Make sure none of the options are empty
            if (dto.Options.Any(o => string.IsNullOrWhiteSpace(o.OptionText)))
            {
                return BadRequest("Options cannot be empty.");
            }
            if (dto.Options
                .GroupBy(o => o.OptionText.Trim().ToLower())
                .Any(g => g.Count() > 1))
            {
                return BadRequest("Options must be unique.");
            }

            var questionBankItem = new QuestionBankItem
            {
                TopicID = dto.TopicID,
                QuestionText = dto.QuestionText,
                SourceExtract = dto.SourceExtract,
                Explanation = dto.Explanation,
                Difficulty = dto.Difficulty,
                Source = "Manual"
            };

            foreach (var optionDto in dto.Options)
            {
                questionBankItem.QuestionBankOptions.Add(
                    new QuestionBankOption
                    {
                        OptionText = optionDto.OptionText,
                        IsCorrect = optionDto.IsCorrect
                    });
            }

            _context.QuestionBankItems.Add(questionBankItem);

            await _context.SaveChangesAsync();

            var result = new QuestionBankItemDto
            {
                QuestionBankItemID = questionBankItem.QuestionBankItemID,
                TopicID = questionBankItem.TopicID,
                TopicName = topic.TopicName,
                QuestionText = questionBankItem.QuestionText,
                SourceExtract = questionBankItem.SourceExtract,
                Explanation = questionBankItem.Explanation,
                Difficulty = questionBankItem.Difficulty,
                Source = questionBankItem.Source,
                Options = questionBankItem.QuestionBankOptions
                    .Select(o => new QuestionBankOptionDto
                    {
                        QuestionBankOptionID = o.QuestionBankOptionID,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList()
            };

            return CreatedAtAction(
                nameof(GetBankItem),
                new { id = questionBankItem.QuestionBankItemID },
                result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankItem(int id)
        {
            var questionBankItem = await _context.QuestionBankItems
                .Include(qbi => qbi.QuestionBankOptions)
                .FirstOrDefaultAsync(qbi => qbi.QuestionBankItemID == id);

            if (questionBankItem == null)
            {
                return NotFound();
            }

            _context.QuestionBankItems.Remove(questionBankItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}