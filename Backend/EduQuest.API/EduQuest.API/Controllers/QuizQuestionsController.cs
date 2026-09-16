using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/quizzes/{quizId}/questions")]
    [ApiController]
    public class QuizQuestionsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        // Layer 2: Business rule
        // GeneratedByAI / ApprovedByAdmin are stored as Yes/No flags for now.
        private static readonly HashSet<string> AllowedYesNo =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Yes",
                "No"
            };

        public QuizQuestionsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/quizzes/{quizId}/questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizQuestionDto>>> GetQuestions(int quizId)
        {
            // Layer 2: Make sure the parent quiz actually exists
            var quizExists = await _context.Quizzes
                .AnyAsync(quiz => quiz.QuizID == quizId);

            if (!quizExists)
            {
                return NotFound("The specified quiz does not exist.");
            }

            var questions = await _context.QuizQuestions
                .Where(question => question.QuizID == quizId)
                .Select(question => new QuizQuestionDto
                {
                    QuizQuestionID = question.QuizQuestionID,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    GeneratedByAI = question.GeneratedByAI,
                    ApprovedByAdmin = question.ApprovedByAdmin,
                    QuizID = question.QuizID
                })
                .ToListAsync();

            return Ok(questions);
        }


        [Authorize(Roles = "Admin")]

        // POST: api/quizzes/{quizId}/questions
        [HttpPost]
        public async Task<ActionResult<QuizQuestionDto>> CreateQuestion(
            int quizId,
            CreateQuizQuestionDto dto)
        {
            // Layer 2: Make sure the parent quiz actually exists
            var quizExists = await _context.Quizzes
                .AnyAsync(quiz => quiz.QuizID == quizId);

            if (!quizExists)
            {
                return NotFound("The specified quiz does not exist.");
            }

            // Normalize the input
            var questionText = dto.QuestionText.Trim();
            var explanation = dto.Explanation.Trim();
            var generatedByAI = dto.GeneratedByAI.Trim();
            var approvedByAdmin = dto.ApprovedByAdmin.Trim();

            // Layer 2: Check that GeneratedByAI is Yes or No
            var validGeneratedByAI = AllowedYesNo.FirstOrDefault(
                v => string.Equals(v, generatedByAI, StringComparison.OrdinalIgnoreCase));

            if (validGeneratedByAI == null)
            {
                return BadRequest("GeneratedByAI must be Yes or No.");
            }

            // Layer 2: Check that ApprovedByAdmin is Yes or No
            var validApprovedByAdmin = AllowedYesNo.FirstOrDefault(
                v => string.Equals(v, approvedByAdmin, StringComparison.OrdinalIgnoreCase));

            if (validApprovedByAdmin == null)
            {
                return BadRequest("ApprovedByAdmin must be Yes or No.");
            }

            // Store canonical versions
            generatedByAI = validGeneratedByAI;
            approvedByAdmin = validApprovedByAdmin;

            var questionEntity = new QuizQuestion
            {
                QuestionText = questionText,
                Explanation = explanation,
                GeneratedByAI = generatedByAI,
                ApprovedByAdmin = approvedByAdmin,
                QuizID = quizId
            };

            _context.QuizQuestions.Add(questionEntity);
            await _context.SaveChangesAsync();

            var result = new QuizQuestionDto
            {
                QuizQuestionID = questionEntity.QuizQuestionID,
                QuestionText = questionEntity.QuestionText,
                Explanation = questionEntity.Explanation,
                GeneratedByAI = questionEntity.GeneratedByAI,
                ApprovedByAdmin = questionEntity.ApprovedByAdmin,
                QuizID = questionEntity.QuizID
            };

            // No single-item GET endpoint exists for questions,
            // so we point the Location header at the list endpoint instead.
            return CreatedAtAction(
                nameof(GetQuestions),
                new { quizId = questionEntity.QuizID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        // PUT: api/questions/{id}
        // Absolute override: this does NOT sit under api/quizzes/{quizId}/questions
        [HttpPut("/api/questions/{id}")]
        public async Task<IActionResult> UpdateQuestion(
            int id,
            UpdateQuizQuestionDto dto)
        {
            var questionEntity = await _context.QuizQuestions.FindAsync(id);

            if (questionEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var questionText = dto.QuestionText.Trim();
            var explanation = dto.Explanation.Trim();
            var generatedByAI = dto.GeneratedByAI.Trim();
            var approvedByAdmin = dto.ApprovedByAdmin.Trim();

            // Layer 2: Check that GeneratedByAI is Yes or No
            var validGeneratedByAI = AllowedYesNo.FirstOrDefault(
                v => string.Equals(v, generatedByAI, StringComparison.OrdinalIgnoreCase));

            if (validGeneratedByAI == null)
            {
                return BadRequest("GeneratedByAI must be Yes or No.");
            }

            // Layer 2: Check that ApprovedByAdmin is Yes or No
            var validApprovedByAdmin = AllowedYesNo.FirstOrDefault(
                v => string.Equals(v, approvedByAdmin, StringComparison.OrdinalIgnoreCase));

            if (validApprovedByAdmin == null)
            {
                return BadRequest("ApprovedByAdmin must be Yes or No.");
            }

            questionEntity.QuestionText = questionText;
            questionEntity.Explanation = explanation;
            questionEntity.GeneratedByAI = validGeneratedByAI;
            questionEntity.ApprovedByAdmin = validApprovedByAdmin;
            // QuizID intentionally left unchanged — see UpdateQuizQuestionDto note above

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        // DELETE: api/questions/{id}
        // Absolute override: this does NOT sit under api/quizzes/{quizId}/questions
        [HttpDelete("/api/questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var questionEntity = await _context.QuizQuestions.FindAsync(id);

            if (questionEntity == null)
            {
                return NotFound();
            }

            _context.QuizQuestions.Remove(questionEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}