using System.Security.Claims;
using EduQuest.API.Data;
using EduQuest.API.DTOs.Learners;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LearnersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LearnersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/learners   (Admin only — full list)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearnerDto>>> GetLearners()
        {
            var learners = await _context.Learners
            .Select(l => new LearnerDto
            {
                LearnerID = l.LearnerID,
                UserID = l.UserID,
                GradeID = l.GradeID,
                GradeName = l.Grade.GradeName,
                SchoolID = l.SchoolID,
                SchoolName = l.School.SchoolName,
                ProvinceID = l.School.ProvinceID,
                ProvinceName = l.School.Province.ProvinceName
            })
             .ToListAsync();

            return Ok(learners);
        }

        // GET: api/learners/me   (the signed-in user's own profile)
        [HttpGet("me")]
        public async Task<ActionResult<LearnerProfileDto>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var profile = await _context.Learners
                .Where(l => l.UserID == currentUserId)
                .Select(l => new LearnerProfileDto
                {
                    LearnerID = l.LearnerID,
                    UserID = l.UserID,
                    FirstName = l.User!.FirstName,
                    LastName = l.User.LastName,
                    Email = l.User.Email,
                    DateCreated = l.User.DateCreated,
                    GradeID = l.GradeID,
                    GradeName = l.Grade.GradeName,
                    SchoolID = l.SchoolID,
                    SchoolName = l.School.SchoolName,
                    ProvinceID = l.School.ProvinceID,
                    ProvinceName = l.School.Province.ProvinceName
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return NotFound("No learner profile exists for this account.");

            return Ok(profile);
        }

        // GET: api/learners/me/quizzes
        // The published quizzes for the signed-in learner's grade and chosen subjects,
        // each with the learner's latest submitted result.
        [HttpGet("me/quizzes")]
        public async Task<ActionResult<IEnumerable<MyQuizDto>>> GetMyQuizzes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var learner = await _context.Learners
                .AsNoTracking()
                .Include(l => l.Grade)
                .FirstOrDefaultAsync(l => l.UserID == currentUserId);

            if (learner == null)
                return NotFound("No learner profile exists for this account.");

            var gradeName = learner.Grade.GradeName;

            var subjectIds = await _context.learnerSubjects
                .Where(ls => ls.LearnerID == learner.LearnerID)
                .Select(ls => ls.SubjectID)
                .ToListAsync();

            // A quiz can only be started once it has 10 approved questions,
            // so only those are listed.
            var quizzes = await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.IsPublished
                    && q.Topic!.GradeLevel == gradeName
                    && subjectIds.Contains(q.Topic.SubjectID)
                    && q.QuizQuestions.Count(x => x.ApprovedByAdmin) >= 10)
                .Select(q => new
                {
                    q.QuizID,
                    q.QuizTitle,
                    q.Difficulty,
                    q.Topic!.SubjectID,
                    q.Topic.Subject!.SubjectName,
                    q.Topic.TopicName
                })
                .ToListAsync();

            var quizIds = quizzes.Select(q => q.QuizID).ToList();

            // Only submitted attempts (ones with saved answers) count as completed.
            var attempts = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.LearnerID == learner.LearnerID
                    && quizIds.Contains(a.QuizID)
                    && a.QuizAttemptAnswers.Any())
                .Select(a => new
                {
                    a.QuizID,
                    a.QuizAttemptID,
                    a.Score,
                    a.DateTaken,
                    TotalQuestions = a.QuizAttemptQuestions.Count
                })
                .ToListAsync();

            var attemptsByQuiz = attempts
                .GroupBy(a => a.QuizID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.DateTaken).ToList());

            var result = quizzes
                .Select(q =>
                {
                    attemptsByQuiz.TryGetValue(q.QuizID, out var mine);
                    var latest = mine?.FirstOrDefault();

                    return new MyQuizDto
                    {
                        QuizID = q.QuizID,
                        QuizTitle = q.QuizTitle,
                        Difficulty = q.Difficulty,
                        SubjectID = q.SubjectID,
                        SubjectName = q.SubjectName,
                        TopicName = q.TopicName,
                        QuestionCount = 10,
                        Completed = latest != null,
                        AttemptCount = mine?.Count ?? 0,
                        LatestAttemptID = latest?.QuizAttemptID,
                        // Score is the number of correct answers, so convert to a percentage.
                        LatestPercentage = latest == null
                            ? null
                            : (latest.TotalQuestions > 0
                                ? (int)Math.Round(latest.Score * 100.0 / latest.TotalQuestions)
                                : 0),
                        LatestDateTaken = latest?.DateTaken
                    };
                })
                .OrderBy(q => q.SubjectName)
                .ThenBy(q => q.QuizTitle)
                .ToList();

            return Ok(result);
        }

        // PUT: api/learners/me   (the signed-in learner edits their own name, grade and school)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var learner = await _context.Learners
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.UserID == currentUserId);

            if (learner == null || learner.User == null)
                return NotFound("No learner profile exists for this account.");

            var firstName = dto.FirstName.Trim();
            var lastName = dto.LastName.Trim();

            if (firstName.Length == 0 || lastName.Length == 0)
                return BadRequest("First name and last name are required.");

            if (await _context.Grades.FindAsync(dto.GradeID) == null)
                return NotFound("Grade not found.");

            if (await _context.Schools.FindAsync(dto.SchoolID) == null)
                return NotFound("School not found.");

            learner.User.FirstName = firstName;
            learner.User.LastName = lastName;
            learner.GradeID = dto.GradeID;
            learner.SchoolID = dto.SchoolID;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/learners/{id}   (Admin, or the learner viewing their own record)
        [HttpGet("{id}")]
        public async Task<ActionResult<LearnerDto>> GetLearner(int id)
        {
            var learner = await _context.Learners
                .Include(l => l.Grade)
                .Include(l => l.School)
                .ThenInclude(s => s.Province)
                .FirstOrDefaultAsync(l => l.LearnerID == id);

            if (learner == null)
                return NotFound();

            if (!IsAdminOrOwner(learner.UserID))
                return Forbid();

            return Ok(new LearnerDto
            {
                LearnerID = learner.LearnerID,
                UserID = learner.UserID,
                GradeID = learner.GradeID,
                GradeName = learner.Grade.GradeName,
                SchoolID = learner.SchoolID,
                SchoolName = learner.School.SchoolName,
                ProvinceID = learner.School.ProvinceID,
                ProvinceName = learner.School.Province.ProvinceName
            });
        }

        // POST: api/learners
        [HttpPost]
        public async Task<ActionResult<LearnerDto>> CreateLearner(CreateLearnerDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserID);
            if (user == null)
                return NotFound("User not found.");

            if (!IsAdminOrOwner(dto.UserID))
                return Forbid();

            var grade = await _context.Grades.FindAsync(dto.GradeID);
            if (grade == null)
                return NotFound("Grade not found.");

            var alreadyExists = await _context.Learners.AnyAsync(l => l.UserID == dto.UserID);
            if (alreadyExists)
                return Conflict("This user already has a learner profile.");

            var school = await _context.Schools
            .Include(s => s.Province)
            .FirstOrDefaultAsync(s => s.SchoolID == dto.SchoolID);

            if (school == null)
                return NotFound("School not found.");

            var learner = new Learner
            {
                UserID = dto.UserID,
                GradeID = dto.GradeID,
                SchoolID = dto.SchoolID
            };

            _context.Learners.Add(learner);
            await _context.SaveChangesAsync();

            var result = new LearnerDto
            {
                LearnerID = learner.LearnerID,
                UserID = learner.UserID,
                GradeID = learner.GradeID,
                GradeName = grade.GradeName,
                SchoolID = learner.SchoolID,
                SchoolName = school.SchoolName,
                ProvinceID = school.ProvinceID,
                ProvinceName = school.Province.ProvinceName
            };

            return CreatedAtAction(nameof(GetLearner), new { id = learner.LearnerID }, result);
        }

        // PUT: api/learners/{id}   (Admin, or the learner updating their own record)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLearner(int id, UpdateLearnerDto dto)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
                return NotFound();

            if (!IsAdminOrOwner(learner.UserID))
                return Forbid();

            var grade = await _context.Grades.FindAsync(dto.GradeID);
            if (grade == null)
                return NotFound("Grade not found.");

            var school = await _context.Schools
             .FindAsync(dto.SchoolID);

            if (school == null)
                return NotFound("School not found.");

            learner.GradeID = dto.GradeID;
            learner.SchoolID = dto.SchoolID;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/learners/{id}   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
                return NotFound();

            _context.Learners.Remove(learner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper: true if caller is Admin, or the UserID in the token matches targetUserId
        private bool IsAdminOrOwner(int targetUserId)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return currentUserId == targetUserId;
        }
    }
}