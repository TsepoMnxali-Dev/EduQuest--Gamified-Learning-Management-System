using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public GradesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/grades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetGrades()
        {
            var grades = await _context.Grades
                .Select(g => new GradeDto
                {
                    GradeID = g.GradeID,
                    GradeName = g.GradeName
                })
                .ToListAsync();

            return Ok(grades);
        }

        // GET: api/grades/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GradeDto>> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Where(g => g.GradeID == id)
                .Select(g => new GradeDto
                {
                    GradeID = g.GradeID,
                    GradeName = g.GradeName
                })
                .FirstOrDefaultAsync();

            if (grade == null)
            {
                return NotFound();
            }

            return Ok(grade);
        }

        [HttpPost]
        public async Task<ActionResult<GradeDto>> CreateGrade(CreateGradeDto dto)
        {
            var grade = new Grade
            {
                GradeName = dto.GradeName
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            var result = new GradeDto
            {
                GradeID = grade.GradeID,
                GradeName = grade.GradeName
            };

            return CreatedAtAction(
                nameof(GetGrade),
                new { id = grade.GradeID },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(int id, UpdateGradeDto dto)
        {
            var grade = await _context.Grades.FindAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            grade.GradeName = dto.GradeName;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}