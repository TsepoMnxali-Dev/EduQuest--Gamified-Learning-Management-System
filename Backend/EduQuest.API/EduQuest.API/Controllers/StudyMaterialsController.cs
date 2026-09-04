using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyMaterialsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public StudyMaterialsController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyMaterialDto>>> GetStudyMaterials()
        {
            var materials = await _context.StudyMaterials
                .Select(sm => new StudyMaterialDto
                {
                    StudyMaterialID = sm.StudyMaterialID,
                    SubjectID = sm.SubjectID,
                    GradeID = sm.GradeID,
                    SubjectName = sm.Subject.SubjectName,
                    GradeName = sm.Grade.GradeName,
                    Title = sm.Title,
                    FileURL = sm.FileURL,
                    ResourceType = sm.ResourceType
                })
                .ToListAsync();

            return Ok(materials);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudyMaterialDto>> GetStudyMaterial(int id)
        {
            var material = await _context.StudyMaterials
                .Where(sm => sm.StudyMaterialID == id)
                .Select(sm => new StudyMaterialDto
                {
                    StudyMaterialID = sm.StudyMaterialID,
                    SubjectID = sm.SubjectID,
                    GradeID = sm.GradeID,
                    SubjectName = sm.Subject.SubjectName,
                    GradeName = sm.Grade.GradeName,
                    Title = sm.Title,
                    FileURL = sm.FileURL,
                    ResourceType = sm.ResourceType
                })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return NotFound();
            }

            return Ok(material);
        }

        [HttpPost]
        public async Task<ActionResult<StudyMaterialDto>> CreateStudyMaterial(
    CreateStudyMaterialDto dto)
        {
            var subject = await _context.Subjects.FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var grade = await _context.Grades.FindAsync(dto.GradeID);

            if (grade == null)
            {
                return NotFound("Grade not found.");
            }

            var material = new StudyMaterial
            {
                SubjectID = dto.SubjectID,
                Subject = subject,
                GradeID = dto.GradeID,
                Grade = grade,
                Title = dto.Title,
                FileURL = dto.FileURL,
                ResourceType = dto.ResourceType
            };

            _context.StudyMaterials.Add(material);
            await _context.SaveChangesAsync();

            var result = new StudyMaterialDto
            {
                StudyMaterialID = material.StudyMaterialID,
                SubjectID = material.SubjectID,
                GradeID = material.GradeID,
                SubjectName = subject.SubjectName,
                GradeName = grade.GradeName,
                Title = material.Title,
                FileURL = material.FileURL,
                ResourceType = material.ResourceType
            };

            return CreatedAtAction(
                nameof(GetStudyMaterial),
                new { id = material.StudyMaterialID },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyMaterial(
    int id,
    UpdateStudyMaterialDto dto)
        {
            var material = await _context.StudyMaterials.FindAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var grade = await _context.Grades.FindAsync(dto.GradeID);

            if (grade == null)
            {
                return NotFound("Grade not found.");
            }

            material.SubjectID = dto.SubjectID;
            material.Subject = subject;
            material.GradeID = dto.GradeID;
            material.Grade = grade;
            material.Title = dto.Title;
            material.FileURL = dto.FileURL;
            material.ResourceType = dto.ResourceType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyMaterial(int id)
        {
            var material = await _context.StudyMaterials.FindAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            _context.StudyMaterials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}