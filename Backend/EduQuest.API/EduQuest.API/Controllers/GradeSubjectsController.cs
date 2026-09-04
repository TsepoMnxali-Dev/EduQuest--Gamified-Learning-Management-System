using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
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
        public async Task<ActionResult<IEnumerable<GradeSubjectDto>>> GetGradeSubjects(int gradeId)
        {
            var gradeExists = await _context.Grades
                .AnyAsync(g => g.GradeID == gradeId);

            if (!gradeExists)
            {
                return NotFound();
            }

            var subjects = await _context.GradeSubjects
                .Where(gs => gs.GradeID == gradeId)
                .Select(gs => new GradeSubjectDto
                {
                    SubjectID = gs.SubjectID,
                    SubjectName = gs.Subject.SubjectName,
                    Description = gs.Description
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpPost("{subjectId}")]
        public async Task<ActionResult<GradeSubjectDto>> AddSubjectToGrade(
    int gradeId,
    int subjectId,
    CreateGradeSubjectDto dto)
        {
            var grade = await _context.Grades.FindAsync(gradeId);

            if (grade == null)
            {
                return NotFound("Grade not found.");
            }

            var subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var alreadyExists = await _context.GradeSubjects
                .AnyAsync(gs => gs.GradeID == gradeId && gs.SubjectID == subjectId);

            if (alreadyExists)
            {
                return Conflict("This subject is already assigned to this grade.");
            }

            var gradeSubject = new GradeSubject
            {
                GradeID = gradeId,
                Grade = grade,
                SubjectID = subjectId,
                Subject = subject,
                Description = dto.Description
            };

            _context.GradeSubjects.Add(gradeSubject);
            await _context.SaveChangesAsync();

            var result = new GradeSubjectDto
            {
                SubjectID = subject.SubjectID,
                SubjectName = subject.SubjectName,
                Description = gradeSubject.Description
            };

            return Ok(result);
        }

        [HttpDelete("{subjectId}")]
        public async Task<IActionResult> RemoveSubjectFromGrade(
    int gradeId,
    int subjectId)
        {
            var gradeSubject = await _context.GradeSubjects
                .FirstOrDefaultAsync(gs =>
                    gs.GradeID == gradeId &&
                    gs.SubjectID == subjectId);

            if (gradeSubject == null)
            {
                return NotFound("This subject is not assigned to this grade.");
            }

            _context.GradeSubjects.Remove(gradeSubject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}