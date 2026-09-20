using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using EduQuest.API.Services;
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
        private readonly GeminiQuizGeneratorService _quizGenerator;

        // Layer 2: Business rule
        // GeneratedByAI / ApprovedByAdmin are stored as Yes/No flags for now.
        private static readonly HashSet<string> AllowedYesNo =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Yes",
                "No"
            };

        public QuizQuestionsController(
            ApplicationDBContext context,
            GeminiQuizGeneratorService quizGenerator)
        {
            _context = context;
            _quizGenerator = quizGenerator;
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

        // ==================================================================
        // POST: api/quizzes/{quizId}/questions/generate
        //
        // What this does, step by step:
        //   1. Find the quiz and its Topic/Subject (we need these for context
        //      even if there's no study material yet).
        //   2. Load the study material files that belong to that Topic (or
        //      just the ones the admin picked, via dto.StudyMaterialIds).
        //   3. Skip any material Gemini can't read (wrong file type, no file
        //      uploaded yet — e.g. link-only material — or too big).
        //   4. Send the readable material's actual file bytes to Gemini and
        //      ask it to write multiple-choice questions FROM that content.
        //   5. Save each generated question with its 4 options, storing which
        //      option is correct (QuizOption.IsCorrect) so learners can later
        //      review what the right answer was.
        //   6. Mark every generated question GeneratedByAI = "Yes" and
        //      ApprovedByAdmin = "Yes" — no manual approval step needed,
        //      the questions are saved and usable in the quiz right away.
        // ==================================================================
        [HttpPost("generate")]
        public async Task<ActionResult<GenerateQuizQuestionsResponseDto>> GenerateQuestions(
            int quizId,
            GenerateQuizQuestionsDto dto)
        {
            // --- Step 1: load the quiz + topic + subject -------------------
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                    .ThenInclude(t => t!.Subject)
                .FirstOrDefaultAsync(q => q.QuizID == quizId);

            if (quiz == null)
            {
                return NotFound("The specified quiz does not exist.");
            }

            if (quiz.Topic == null || quiz.Topic.Subject == null)
            {
                return BadRequest("The quiz's topic/subject must be set before generating questions.");
            }

            var difficulty = string.IsNullOrWhiteSpace(dto.Difficulty)
                ? quiz.Difficulty
                : dto.Difficulty.Trim();

            // Notes we collect along the way and hand back to the admin, e.g.
            // "skipped 1 file (unsupported type)". Doesn't stop the request —
            // it's just visibility into what Gemini actually saw.
            var notes = new List<string>();

            // --- Step 2: find candidate study material ----------------------
            // Materials belong to a Topic (StudyMaterial.TopicID), not directly
            // to a Quiz, so we look them up by the quiz's TopicID. If the admin
            // passed specific StudyMaterialIds, narrow to just those.
            var materialsQuery = _context.StudyMaterials
                .Where(sm => sm.TopicID == quiz.TopicID);

            if (dto.StudyMaterialIds != null && dto.StudyMaterialIds.Count > 0)
            {
                materialsQuery = materialsQuery.Where(sm => dto.StudyMaterialIds.Contains(sm.StudyMaterialID));
            }

            var candidateMaterials = await materialsQuery.ToListAsync();

            // --- Step 3: filter down to material Gemini can actually read ---
            var usableMaterials = new List<StudyMaterialInput>();
            long runningBytes = 0;

            foreach (var material in candidateMaterials)
            {
                // Link-only material (a URL with no uploaded file) can't be
                // fetched by us reliably, so we skip it rather than guess.
                if (material.FileData == null || material.FileData.Length == 0)
                {
                    notes.Add($"Skipped \"{material.Title}\" — it's a link, not an uploaded file.");
                    continue;
                }

                var mimeType = material.FileContentType ?? string.Empty;
                if (!GeminiQuizGeneratorService.SupportedMimeTypes.Contains(mimeType))
                {
                    notes.Add($"Skipped \"{material.Title}\" — file type ({mimeType}) isn't supported for AI reading yet.");
                    continue;
                }

                if (!GeminiQuizGeneratorService.FitsWithinBudget(runningBytes, material.FileData.Length))
                {
                    notes.Add($"Skipped \"{material.Title}\" — combined file size for this request got too large.");
                    continue;
                }

                usableMaterials.Add(new StudyMaterialInput(material.Title, material.FileData, mimeType));
                runningBytes += material.FileData.Length;
            }

            if (usableMaterials.Count == 0)
            {
                notes.Add("No usable study material found for this topic — questions were generated from the topic name only.");
            }

            // --- Step 4: ask Gemini to generate the questions ---------------
            List<GeneratedQuestion> generated;
            try
            {
                generated = await _quizGenerator.GenerateQuestionsAsync(
                    subjectName: quiz.Topic.Subject.SubjectName,
                    topicName: quiz.Topic.TopicName,
                    gradeLevel: quiz.Topic.GradeLevel,
                    difficulty: difficulty,
                    count: dto.Count,
                    materials: usableMaterials);
            }
            catch (QuizGenerationException ex)
            {
                // 502 Bad Gateway = "we reached out to another service (Gemini)
                // and it failed", which is more accurate than a generic 500.
                return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
            }

            // --- Step 5: save each question with its options -----------------
            var savedQuestions = new List<QuizQuestion>();

            foreach (var q in generated)
            {
                var questionEntity = new QuizQuestion
                {
                    QuestionText = q.QuestionText.Trim(),
                    Explanation = q.Explanation.Trim(),

                    // Step 6: flag as AI-generated. ApprovedByAdmin is set to
                    // "Yes" straight away — no manual review step, the
                    // question is live and usable in the quiz immediately.
                    GeneratedByAI = "Yes",
                    ApprovedByAdmin = "Yes",
                    QuizID = quizId,

                    // This is where the correct answer gets stored: each
                    // QuizOption keeps its own IsCorrect flag, exactly like
                    // options created by hand through the normal endpoints.
                    // Nothing extra needed for "review your work" later —
                    // the learner-facing review screen just needs to read
                    // IsCorrect off the option the learner picked (and the
                    // one that was actually correct) once the attempt is over.
                    QuizOptions = q.Options
                        .Select(o => new QuizOption
                        {
                            OptionText = o.OptionText.Trim(),
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                };

                _context.QuizQuestions.Add(questionEntity);
                savedQuestions.Add(questionEntity);
            }

            await _context.SaveChangesAsync();

            // --- Build the response ------------------------------------------
            var questionDtos = savedQuestions.Select(question => new GeneratedQuizQuestionDto
            {
                QuizQuestionID = question.QuizQuestionID,
                QuestionText = question.QuestionText,
                Explanation = question.Explanation,
                GeneratedByAI = question.GeneratedByAI,
                ApprovedByAdmin = question.ApprovedByAdmin,
                QuizID = question.QuizID,
                Options = question.QuizOptions!
                    .Select(o => new QuizOptionDto
                    {
                        QuizOptionID = o.QuizOptionID,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect,
                        QuestionID = o.QuestionID
                    })
                    .ToList()
            }).ToList();

            return Ok(new GenerateQuizQuestionsResponseDto
            {
                Questions = questionDtos,
                Notes = notes
            });
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