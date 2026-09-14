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
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        // Layer 2: Business rule
        // EduQuest only supports Grades 10–12 for the current version.
        private static readonly HashSet<string> AllowedGrades =
            new(StringComparer.OrdinalIgnoreCase)
            {
          
                "Grade 10",
                "Grade 11",
                "Grade 12"
            };

        public GradesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/grades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetGrades()
        {
            var grades = await _context.Grades
                .Select(grade => new GradeDto
                {
                    GradeID = grade.GradeID,
                    GradeName = grade.GradeName
                })
                .ToListAsync();

            return Ok(grades);
        }

        // GET: api/grades/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GradeDto>> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Where(grade => grade.GradeID == id)
                .Select(grade => new GradeDto
                {
                    GradeID = grade.GradeID,
                    GradeName = grade.GradeName
                })
                .FirstOrDefaultAsync();

            if (grade == null)
            {
                return NotFound();
            }

            return Ok(grade);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<GradeDto>> CreateGrade(CreateGradeDto dto)
        {
            // Normalize the input
            var gradeName = dto.GradeName.Trim();

            // Layer 2: Check that the grade is Grade 8–12
            var validGrade = AllowedGrades.FirstOrDefault(
                grade => string.Equals(
                    grade,
                    gradeName,
                    StringComparison.OrdinalIgnoreCase));

            if (validGrade == null)
            {
                return BadRequest(
                    "Grade must be Grade 10, Grade 11, or Grade 12.");
            }

            // Store the canonical version
            // e.g. "grade 8" becomes "Grade 8"
            gradeName = validGrade;

            // Layer 2: Prevent duplicate grades
            var exists = await _context.Grades
                .AnyAsync(grade => grade.GradeName == gradeName);

            if (exists)
            {
                return Conflict("This grade already exists.");
            }

            var gradeEntity = new Grade
            {
                GradeName = gradeName
            };

            _context.Grades.Add(gradeEntity);
            await _context.SaveChangesAsync();

            var result = new GradeDto
            {
                GradeID = gradeEntity.GradeID,
                GradeName = gradeEntity.GradeName
            };

            return CreatedAtAction(
                nameof(GetGrade),
                new { id = gradeEntity.GradeID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(
            int id,
            UpdateGradeDto dto)
        {
            var gradeEntity = await _context.Grades.FindAsync(id);

            if (gradeEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var gradeName = dto.GradeName.Trim();

            // Layer 2: Check that the grade is Grade 8–12
            var validGrade = AllowedGrades.FirstOrDefault(
                grade => string.Equals(
                    grade,
                    gradeName,
                    StringComparison.OrdinalIgnoreCase));

            if (validGrade == null)
            {
                return BadRequest(
                    "Grade must be Grade 10, Grade 11, or Grade 12.");
            }

            // Store the canonical version
            gradeName = validGrade;

            // Layer 2: Prevent duplicate grades
            // Exclude the current grade from the check.
            var exists = await _context.Grades
                .AnyAsync(grade =>
                    grade.GradeName == gradeName &&
                    grade.GradeID != id);

            if (exists)
            {
                return Conflict("This grade already exists.");
            }

            gradeEntity.GradeName = gradeName;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var gradeEntity = await _context.Grades.FindAsync(id);

            if (gradeEntity == null)
            {
                return NotFound();
            }

            _context.Grades.Remove(gradeEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}