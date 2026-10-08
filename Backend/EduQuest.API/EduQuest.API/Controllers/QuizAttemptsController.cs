using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        [Authorize(Roles = "Admin")]
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

        // GET: api/quizattempts/{attemptId}/result
        [HttpGet("{attemptId}/result")]
        public async Task<ActionResult<QuizAttemptResultDto>> GetQuizAttemptResult(
            int attemptId)
        {
            // ---------------------------------------------------------------
            // 1. Get the completed attempt
            // ---------------------------------------------------------------
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.QuizAttemptQuestions)
                    .ThenInclude(aq => aq.QuizQuestion)
                        .ThenInclude(q => q!.QuizOptions)
                .Include(a => a.QuizAttemptAnswers)
                .FirstOrDefaultAsync(a => a.QuizAttemptID == attemptId);

            if (attempt == null)
            {
                return NotFound("The specified quiz attempt does not exist.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var loggedInUserId))
            {
                return Unauthorized();
            }

            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.UserID == loggedInUserId);

            if (learner == null)
            {
                return Unauthorized();
            }

            if (attempt.LearnerID != learner.LearnerID)
            {
                return Forbid();
            }

            // ---------------------------------------------------------------
            // 2. Make sure the attempt has been submitted
            // ---------------------------------------------------------------
            var submittedAnswers = await _context.QuizAttemptAnswers
                .Where(answer => answer.AttemptID == attemptId)
                .ToListAsync();

            if (submittedAnswers.Count == 0)
            {
                return BadRequest("This quiz attempt has not been submitted yet.");
            }

            // ---------------------------------------------------------------
            // 3. Calculate the total questions and percentage
            // ---------------------------------------------------------------
            var totalQuestions = attempt.QuizAttemptQuestions.Count;

            var percentage = totalQuestions > 0
                ? (attempt.Score / (double)totalQuestions) * 100
                : 0;

            // ---------------------------------------------------------------
            // 4. Build the question results
            // ---------------------------------------------------------------
            var questionResults = new List<QuizAttemptResultQuestionDto>();

            foreach (var attemptQuestion in attempt.QuizAttemptQuestions
                .OrderBy(q => q.QuestionOrder))
            {
                var question = attemptQuestion.QuizQuestion!;

                var submittedAnswer = submittedAnswers
                    .FirstOrDefault(answer =>
                        answer.QuestionID == question.QuizQuestionID);

                var selectedOption = question.QuizOptions
                    .FirstOrDefault(option =>
                        option.QuizOptionID == submittedAnswer!.QuizOptionID);

                var correctOption = question.QuizOptions
                    .FirstOrDefault(option => option.IsCorrect);

                questionResults.Add(new QuizAttemptResultQuestionDto
                {
                    QuestionOrder = attemptQuestion.QuestionOrder,

                    QuestionText = question.QuestionText,

                    SelectedAnswer = selectedOption!.OptionText,

                    CorrectAnswer = correctOption!.OptionText,

                    IsCorrect = selectedOption.IsCorrect,

                    Explanation = question.Explanation
                });
            }

            // ---------------------------------------------------------------
            // 5. Build the complete result
            // ---------------------------------------------------------------
            var result = new QuizAttemptResultDto
            {
                QuizAttemptID = attempt.QuizAttemptID,
                QuizID = attempt.QuizID,
                QuizTitle = attempt.Quiz!.QuizTitle,
                LearnerID = attempt.LearnerID,

                Score = attempt.Score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,

                DateTaken = attempt.DateTaken,
                TimeTaken = attempt.TimeTaken,

                Questions = questionResults
            };

            return Ok(result);
        }

        // POST: api/quizzes/{quizId}/start
        [HttpPost("/api/quizzes/{quizId}/start")]
        public async Task<ActionResult<StartQuizResponseDto>> StartQuiz(
            int quizId)
        {
            // ---------------------------------------------------------------
            // 1. Make sure the quiz exists
            // ---------------------------------------------------------------
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.QuizID == quizId);

            if (quiz == null)
            {
                return NotFound("The specified quiz does not exist.");
            }

            // ---------------------------------------------------------------
            // 2. Only published quizzes can be started by learners
            // ---------------------------------------------------------------
            if (!quiz.IsPublished)
            {
                return BadRequest("This quiz is not currently published.");
            }

            // ---------------------------------------------------------------
            // 3. Make sure the learner exists
            // ---------------------------------------------------------------
            // Get the logged-in user's UserID from the JWT.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var loggedInUserId))
            {
                return Unauthorized();
            }

            // Find the learner belonging to the logged-in user.
            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.UserID == loggedInUserId);

            if (learner == null)
            {
                return Unauthorized();
            }

            // ---------------------------------------------------------------
            // 4. Get all approved questions for this quiz
            // ---------------------------------------------------------------
            var approvedQuestions = await _context.QuizQuestions
                .Where(question =>
                    question.QuizID == quizId &&
                    question.ApprovedByAdmin)
                .Include(question => question.QuizOptions)
                .ToListAsync();

            // ---------------------------------------------------------------
            // 5. A quiz must have at least 10 approved questions
            // ---------------------------------------------------------------
            if (approvedQuestions.Count < 10)
            {
                return BadRequest(
                    $"This quiz does not have enough approved questions. " +
                    $"At least 10 are required, but only {approvedQuestions.Count} are available.");
            }

            // ---------------------------------------------------------------
            // 6. Randomly select exactly 10 questions
            // ---------------------------------------------------------------
            var selectedQuestions = approvedQuestions
                .OrderBy(_ => Guid.NewGuid())
                .Take(10)
                .ToList();

            // ---------------------------------------------------------------
            // 7. Create the quiz attempt
            // ---------------------------------------------------------------
            var attempt = new QuizAttempt
            {
                Score = 0,
                DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                TimeTaken = "00:00:00",
                QuizID = quizId,
                LearnerID = learner.LearnerID
            };

            _context.QuizAttempts.Add(attempt);

            // ---------------------------------------------------------------
            // 8. Record which questions were assigned to this attempt
            // ---------------------------------------------------------------
            for (int i = 0; i < selectedQuestions.Count; i++)
            {
                var attemptQuestion = new QuizAttemptQuestion
                {
                    QuizAttempt = attempt,
                    QuizQuestionID = selectedQuestions[i].QuizQuestionID,
                    QuestionOrder = i + 1
                };

                _context.QuizAttemptQuestions.Add(attemptQuestion);
            }

            // Save the attempt and its assigned questions
            await _context.SaveChangesAsync();

            // ---------------------------------------------------------------
            // 9. Build the learner-safe response
            // ---------------------------------------------------------------
            var questionDtos = selectedQuestions
                .Select((question, index) => new QuizAttemptQuestionDto
                {
                    QuizAttemptQuestionID = attempt.QuizAttemptQuestions
                        .First(q => q.QuizQuestionID == question.QuizQuestionID)
                        .QuizAttemptQuestionID,

                    QuizQuestionID = question.QuizQuestionID,

                    QuestionOrder = index + 1,

                    QuestionText = question.QuestionText,

                    SourceExtract = question.SourceExtract,

                    Options = question.QuizOptions!
                        .Select(option => new QuizAttemptOptionDto
                        {
                            QuizOptionID = option.QuizOptionID,
                            OptionText = option.OptionText
                        })
                        .ToList()
                })
                .ToList();

            // ---------------------------------------------------------------
            // 10. Return the started quiz
            // ---------------------------------------------------------------
            var response = new StartQuizResponseDto
            {
                QuizAttemptID = attempt.QuizAttemptID,
                QuizID = quiz.QuizID,
                LearnerID = attempt.LearnerID,
                QuizTitle = quiz.QuizTitle,
                Difficulty = quiz.Difficulty,
                TimeLimit = quiz.TimeLimit,
                Questions = questionDtos
            };

            return Ok(response);
        }

       
            // POST: api/quizattempts/{attemptId}/submit
            [HttpPost("{attemptId}/submit")]
            public async Task<ActionResult<QuizAttemptDto>> SubmitQuizAttempt(
                int attemptId,
                SubmitQuizAttemptDto dto)
                    {
            // ---------------------------------------------------------------
            // 1. Make sure the attempt exists
            // ---------------------------------------------------------------
            var attempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.QuizAttemptID == attemptId);

            if (attempt == null)
            {
                return NotFound("The specified quiz attempt does not exist.");
            }
            // Get the logged-in user's UserID from the JWT.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var loggedInUserId))
            {
                return Unauthorized();
            }

            // Find the learner belonging to the logged-in user.
            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.UserID == loggedInUserId);

            if (learner == null)
            {
                return Unauthorized();
            }

            // Make sure the attempt belongs to the logged-in learner.
            if (attempt.LearnerID != learner.LearnerID)
            {
                return Forbid();
            }

            // ---------------------------------------------------------------
            // 2. Make sure the attempt has not already been submitted
            // ---------------------------------------------------------------
            var existingAnswers = await _context.QuizAttemptAnswers
                .AnyAsync(a => a.AttemptID == attemptId);

            if (existingAnswers)
            {
                return BadRequest("This quiz attempt has already been submitted.");
            }

            // ---------------------------------------------------------------
            // 3. Get the questions assigned to this attempt
            // ---------------------------------------------------------------
            var attemptQuestions = await _context.QuizAttemptQuestions
                .Where(aq => aq.QuizAttemptID == attemptId)
                .Include(aq => aq.QuizQuestion)
                .ThenInclude(q => q!.QuizOptions)
                .ToListAsync();

            if (attemptQuestions.Count == 0)
            {
                return BadRequest("This quiz attempt has no assigned questions.");
            }

            // ---------------------------------------------------------------
            // 4. Make sure the learner submitted an answer for every question
            // ---------------------------------------------------------------
            if (dto.Answers.Count != attemptQuestions.Count)
            {
                return BadRequest(
                    $"This quiz requires exactly {attemptQuestions.Count} answers.");
            }

            // ---------------------------------------------------------------
            // 5. Validate that each question is answered only once
            // ---------------------------------------------------------------
            if (dto.Answers
                .GroupBy(a => a.QuizAttemptQuestionID)
                .Any(g => g.Count() > 1))
            {
                return BadRequest("A question cannot be answered more than once.");
            }

            int correctAnswers = 0;

            // ---------------------------------------------------------------
            // 6. Process each submitted answer
            // ---------------------------------------------------------------
            foreach (var submittedAnswer in dto.Answers)
            {
                var attemptQuestion = attemptQuestions
                    .FirstOrDefault(
                        aq => aq.QuizAttemptQuestionID ==
                              submittedAnswer.QuizAttemptQuestionID);

                if (attemptQuestion == null)
                {
                    return BadRequest(
                        $"QuizAttemptQuestionID {submittedAnswer.QuizAttemptQuestionID} " +
                        "does not belong to this quiz attempt.");
                }

                var selectedOption = attemptQuestion.QuizQuestion!
                    .QuizOptions
                    .FirstOrDefault(
                        option => option.QuizOptionID ==
                                  submittedAnswer.QuizOptionID);

                if (selectedOption == null)
                {
                    return BadRequest(
                        $"QuizOptionID {submittedAnswer.QuizOptionID} " +
                        "does not belong to the specified question.");
                }

                if (selectedOption.IsCorrect)
                {
                    correctAnswers++;
                }

                var answer = new QuizAttemptAnswer
                {
                    AttemptID = attemptId,
                    QuestionID = attemptQuestion.QuizQuestionID,
                    QuizOptionID = selectedOption.QuizOptionID
                };

                _context.QuizAttemptAnswers.Add(answer);
            }

            // ---------------------------------------------------------------
            // 7. Calculate the score
            // ---------------------------------------------------------------
            attempt.Score = correctAnswers;

            // ---------------------------------------------------------------
            // 8. Save the answers and updated score
            // ---------------------------------------------------------------
            await _context.SaveChangesAsync();

            // ---------------------------------------------------------------
            // 9. Return the completed attempt
            // ---------------------------------------------------------------
            var result = new QuizAttemptDto
            {
                QuizAttemptID = attempt.QuizAttemptID,
                Score = attempt.Score,
                DateTaken = attempt.DateTaken,
                TimeTaken = attempt.TimeTaken,
                QuizID = attempt.QuizID,
                LearnerID = attempt.LearnerID
            };

            return Ok(result);
        }

        // GET: api/quizattempts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAttemptDto>> GetQuizAttempt(int id)
        {
            // ---------------------------------------------------------------
            // 1. Get the quiz attempt
            // ---------------------------------------------------------------
            var attempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(attempt => attempt.QuizAttemptID == id);

            if (attempt == null)
            {
                return NotFound();
            }

            // ---------------------------------------------------------------
            // 2. Get the logged-in user's UserID from the JWT
            // ---------------------------------------------------------------
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var loggedInUserId))
            {
                return Unauthorized();
            }

            // ---------------------------------------------------------------
            // 3. Find the Learner belonging to the logged-in user
            // ---------------------------------------------------------------
            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.UserID == loggedInUserId);

            if (learner == null)
            {
                return Unauthorized();
            }

            // ---------------------------------------------------------------
            // 4. Make sure the attempt belongs to this learner
            // ---------------------------------------------------------------
            if (attempt.LearnerID != learner.LearnerID)
            {
                return Forbid();
            }

            // ---------------------------------------------------------------
            // 5. Return the attempt
            // ---------------------------------------------------------------
            var result = new QuizAttemptDto
            {
                QuizAttemptID = attempt.QuizAttemptID,
                Score = attempt.Score,
                DateTaken = attempt.DateTaken,
                TimeTaken = attempt.TimeTaken,
                QuizID = attempt.QuizID,
                LearnerID = attempt.LearnerID
            };

            return Ok(result);
        }


        // POST: api/quizattempts
        // No [Authorize(Roles = "Admin")] here — any authenticated learner
        // needs to be able to submit their own attempt.


        // GET: api/learners/{learnerId}/quizattempts
        // Absolute override: this does NOT sit under api/quizattempts
        [HttpGet("/api/learners/{learnerId}/quizattempts")]
        public async Task<ActionResult<IEnumerable<QuizAttemptDto>>> GetQuizAttemptsForLearner(int learnerId)
        {
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == learnerId);

            if (!learnerExists)
            {
                return NotFound("The specified learner does not exist.");
            }

            // Admins can view any learner's attempts.
            if (User.IsInRole("Admin"))
            {
                var adminAttempts = await _context.QuizAttempts
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

                return Ok(adminAttempts);
            }

            // Get the logged-in user's UserID from the JWT.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var loggedInUserId))
            {
                return Unauthorized();
            }

            // Find the learner belonging to the logged-in user.
            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.UserID == loggedInUserId);

            if (learner == null)
            {
                return Unauthorized();
            }

            // Learners can only view their own attempts.
            if (learner.LearnerID != learnerId)
            {
                return Forbid();
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