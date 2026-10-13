
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

        // ==========================================
        // GET: api/learners
        // Admin only - full learner list
        // ==========================================

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

        // ==========================================
        // NEW: GET api/learners/me
        // ==========================================
        // Retrieves the authenticated learner's
        // profile using the UserID from their JWT.
        //
        // Returns:
        // - LearnerID
        // - UserID
        // - GradeID and GradeName
        // - SchoolID and SchoolName
        // - ProvinceID and ProvinceName
        //
        // Only the logged-in learner can retrieve
        // their own profile through this endpoint.
        // ==========================================

        [Authorize(Roles = "Learner")]
        [HttpGet("me")]
        public async Task<ActionResult<LearnerDto>> GetMyProfile()
        {
            // NEW: Get the authenticated UserID.
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // NEW: Find the learner associated
            // with the authenticated user.
            var learner = await _context.Learners
                .AsNoTracking()
                .Where(l => l.UserID == userId)
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
                .FirstOrDefaultAsync();

            if (learner == null)
            {
                return NotFound(
                    "Learner profile not found."
                );
            }

            return Ok(learner);
        }

        // ==========================================
        // GET: api/learners/{id}
        // Admin or learner viewing own record
        // ==========================================

        [HttpGet("{id}")]
        public async Task<ActionResult<LearnerDto>> GetLearner(int id)
        {
            var learner = await _context.Learners
                .Include(l => l.Grade)
                .Include(l => l.School)
                    .ThenInclude(s => s.Province)
                .FirstOrDefaultAsync(l => l.LearnerID == id);

            if (learner == null)
            {
                return NotFound();
            }

            if (!IsAdminOrOwner(learner.UserID))
            {
                return Forbid();
            }

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

        // ==========================================
        // POST: api/learners
        // Create a learner profile
        // ==========================================

        [HttpPost]
        public async Task<ActionResult<LearnerDto>> CreateLearner(
            CreateLearnerDto dto)
        {
            var user = await _context.Users
                .FindAsync(dto.UserID);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!IsAdminOrOwner(dto.UserID))
            {
                return Forbid();
            }

            var grade = await _context.Grades
                .FindAsync(dto.GradeID);

            if (grade == null)
            {
                return NotFound("Grade not found.");
            }

            var alreadyExists = await _context.Learners
                .AnyAsync(l => l.UserID == dto.UserID);

            if (alreadyExists)
            {
                return Conflict(
                    "This user already has a learner profile."
                );
            }

            var school = await _context.Schools
                .Include(s => s.Province)
                .FirstOrDefaultAsync(
                    s => s.SchoolID == dto.SchoolID
                );

            if (school == null)
            {
                return NotFound("School not found.");
            }

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

            return CreatedAtAction(
                nameof(GetLearner),
                new { id = learner.LearnerID },
                result
            );
        }

        // ==========================================
        // PUT: api/learners/{id}
        // Admin or learner updating own record
        // ==========================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLearner(
            int id,
            UpdateLearnerDto dto)
        {
            var learner = await _context.Learners
                .FindAsync(id);

            if (learner == null)
            {
                return NotFound();
            }

            if (!IsAdminOrOwner(learner.UserID))
            {
                return Forbid();
            }

            var grade = await _context.Grades
                .FindAsync(dto.GradeID);

            if (grade == null)
            {
                return NotFound("Grade not found.");
            }

            var school = await _context.Schools
                .FindAsync(dto.SchoolID);

            if (school == null)
            {
                return NotFound("School not found.");
            }

            learner.GradeID = dto.GradeID;
            learner.SchoolID = dto.SchoolID;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE: api/learners/{id}
        // Admin only
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            var learner = await _context.Learners
                .FindAsync(id);

            if (learner == null)
            {
                return NotFound();
            }

            _context.Learners.Remove(learner);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // AUTHORIZATION HELPER
        // ==========================================
        // Admins can access any learner.
        // Learners can access their own record.
        // ==========================================

        private bool IsAdminOrOwner(int targetUserId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // CHANGED:
            // Safely parse the authenticated UserID.
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return false;
            }

            return currentUserId == targetUserId;
        }
    }
}
