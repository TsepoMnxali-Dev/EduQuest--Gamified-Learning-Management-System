using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public SubjectsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectDto>>> GetSubjects()
        {
            var subjects = await _context.Subjects
                .Select(s => new SubjectDto
                {
                    SubjectID = s.SubjectID,
                    SubjectName = s.SubjectName,
                    GradeLevel = s.GradeLevel
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectDto>> GetSubject(int id)
        {
            var subject = await _context.Subjects
                .Where(s => s.SubjectID == id)
                .Select(s => new SubjectDto
                {
                    SubjectID = s.SubjectID,
                    SubjectName = s.SubjectName,
                    GradeLevel = s.GradeLevel
                })
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                return NotFound();
            }

            return Ok(subject);
        }

        [HttpPost]
        public async Task<ActionResult<SubjectDto>> CreateSubject(CreateSubjectDto dto)
        {
            var subject = new Subject
            {
                SubjectName = dto.SubjectName,
                GradeLevel = dto.GradeLevel
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var result = new SubjectDto
            {
                SubjectID = subject.SubjectID,
                SubjectName = subject.SubjectName,
                GradeLevel = subject.GradeLevel
            };

            return CreatedAtAction(
                nameof(GetSubject),
                new { id = subject.SubjectID },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            subject.SubjectName = dto.SubjectName;
            subject.GradeLevel = dto.GradeLevel;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}