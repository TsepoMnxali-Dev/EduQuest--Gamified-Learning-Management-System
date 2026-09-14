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
    public class SubjectsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        // Layer 2: EduQuest's currently supported subjects
        private static readonly HashSet<string> AllowedSubjects =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Mathematics",
                "English",
                "Physical Sciences",
                "Life Sciences",
                "Geography",
                "Life Orientation"
            };

        public SubjectsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectDto>>> GetSubjects()
        {
            var subjects = await _context.Subjects
                .Select(subject => new SubjectDto
                {
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName
                })
                .ToListAsync();

            return Ok(subjects);
        }

        // GET: api/subjects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectDto>> GetSubject(int id)
        {
            var subject = await _context.Subjects
                .Where(subject => subject.SubjectID == id)
                .Select(subject => new SubjectDto
                {
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName
                })
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                return NotFound();
            }

            return Ok(subject);
        }

        // POST: api/subjects
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SubjectDto>> CreateSubject(
            CreateSubjectDto dto)
        {
            // Normalize input
            var subjectName = dto.SubjectName.Trim();

            // Layer 2: Check whether the subject is supported by EduQuest
            var validSubject = AllowedSubjects.FirstOrDefault(
                subject => string.Equals(
                    subject,
                    subjectName,
                    StringComparison.OrdinalIgnoreCase));

            if (validSubject == null)
            {
                return BadRequest(
                    "Subject must be Mathematics, English, Physical Sciences, " +
                    "Life Sciences, Geography, or Life Orientation.");
            }

            // Store the canonical version
            subjectName = validSubject;

            // Layer 2: Prevent duplicate subjects
            var exists = await _context.Subjects
                .AnyAsync(subject => subject.SubjectName == subjectName);

            if (exists)
            {
                return Conflict("This subject already exists.");
            }

            var subjectEntity = new Subject
            {
                SubjectName = subjectName
            };

            _context.Subjects.Add(subjectEntity);
            await _context.SaveChangesAsync();

            var result = new SubjectDto
            {
                SubjectID = subjectEntity.SubjectID,
                SubjectName = subjectEntity.SubjectName
            };

            return CreatedAtAction(
                nameof(GetSubject),
                new { id = subjectEntity.SubjectID },
                result
            );
        }

        // PUT: api/subjects/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(
            int id,
            UpdateSubjectDto dto)
        {
            var subjectEntity = await _context.Subjects.FindAsync(id);

            if (subjectEntity == null)
            {
                return NotFound();
            }

            // Normalize input
            var subjectName = dto.SubjectName.Trim();

            // Layer 2: Check whether the subject is supported by EduQuest
            var validSubject = AllowedSubjects.FirstOrDefault(
                subject => string.Equals(
                    subject,
                    subjectName,
                    StringComparison.OrdinalIgnoreCase));

            if (validSubject == null)
            {
                return BadRequest(
                    "Subject must be Mathematics, English, Physical Sciences, " +
                    "Life Sciences, Geography, or Life Orientation.");
            }

            // Store the canonical version
            subjectName = validSubject;

            // Layer 2: Prevent duplicate subjects
            // Exclude the subject currently being updated.
            var exists = await _context.Subjects
                .AnyAsync(subject =>
                    subject.SubjectName == subjectName &&
                    subject.SubjectID != id);

            if (exists)
            {
                return Conflict("This subject already exists.");
            }

            subjectEntity.SubjectName = subjectName;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/subjects/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subjectEntity = await _context.Subjects.FindAsync(id);

            if (subjectEntity == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subjectEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}