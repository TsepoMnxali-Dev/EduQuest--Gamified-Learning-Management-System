using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/grades/{gradeId}/subjects")]
    [ApiController]
    public class GradeSubjectsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public GradeSubjectsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/grades/{gradeId}/subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeSubjectDto>>> GetGradeSubjects(
            int gradeId)
        {
            // Layer 2: Make sure the grade exists
            var gradeExists = await _context.Grades
                .AnyAsync(grade => grade.GradeID == gradeId);

            if (!gradeExists)
            {
                return NotFound("Grade not found.");
            }

            var subjects = await _context.GradeSubjects
                .Where(gradeSubject => gradeSubject.GradeID == gradeId)
                .Select(gradeSubject => new GradeSubjectDto
                {
                    SubjectID = gradeSubject.SubjectID,
                    SubjectName = gradeSubject.Subject.SubjectName
                })
                .ToListAsync();

            return Ok(subjects);
        }


        // POST: api/grades/{gradeId}/subjects/{subjectId}
        [Authorize(Roles = "Admin")]
        [HttpPost("{subjectId}")]
        public async Task<ActionResult<GradeSubjectDto>> AddSubjectToGrade(
            int gradeId,
            int subjectId)

        {
            // Layer 2: Make sure the grade exists
            var gradeExists = await _context.Grades
                .AnyAsync(grade => grade.GradeID == gradeId);

            if (!gradeExists)
            {
                return NotFound("Grade not found.");
            }

            // Layer 2: Make sure the subject exists
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(subject => subject.SubjectID == subjectId);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Layer 2: Prevent duplicate relationships
            var alreadyExists = await _context.GradeSubjects
                .AnyAsync(gradeSubject =>
                    gradeSubject.GradeID == gradeId &&
                    gradeSubject.SubjectID == subjectId);

            if (alreadyExists)
            {
                return Conflict(
                    "This subject is already assigned to this grade.");
            }

            var gradeSubject = new GradeSubject
            {
                GradeID = gradeId,
                SubjectID = subjectId
            };

            _context.GradeSubjects.Add(gradeSubject);

            await _context.SaveChangesAsync();

            var result = new GradeSubjectDto
            {
                SubjectID = subject.SubjectID,
                SubjectName = subject.SubjectName
            };

            return CreatedAtAction(
                nameof(GetGradeSubjects),
                new { gradeId = gradeId },
                result
            );
        }


        // DELETE: api/grades/{gradeId}/subjects/{subjectId}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{subjectId}")]
        public async Task<IActionResult> RemoveSubjectFromGrade(
            int gradeId,
            int subjectId)
        {
            var gradeSubject = await _context.GradeSubjects
                .FirstOrDefaultAsync(gradeSubject =>
                    gradeSubject.GradeID == gradeId &&
                    gradeSubject.SubjectID == subjectId);

            if (gradeSubject == null)
            {
                return NotFound(
                    "This subject is not assigned to this grade.");
            }

            _context.GradeSubjects.Remove(gradeSubject);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}