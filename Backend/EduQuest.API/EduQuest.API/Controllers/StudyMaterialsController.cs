using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize]
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
                    TopicID = sm.TopicID,
                    TopicName = sm.Topic.TopicName,
                    SubjectID = sm.Topic.SubjectID,
                    SubjectName = sm.Topic.Subject.SubjectName,
                    GradeLevel = sm.Topic.GradeLevel,
                    Title = sm.Title,
                    Description = sm.Description,
                    ResourceType = sm.ResourceType,
                    FileName = sm.FileName,
                    FileURL = sm.FileURL
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
                    TopicID = sm.TopicID,
                    TopicName = sm.Topic.TopicName,
                    SubjectID = sm.Topic.SubjectID,
                    SubjectName = sm.Topic.Subject.SubjectName,
                    GradeLevel = sm.Topic.GradeLevel,
                    Title = sm.Title,
                    Description = sm.Description,
                    ResourceType = sm.ResourceType,
                    FileName = sm.FileName,
                    FileURL = sm.FileURL
                })
                .FirstOrDefaultAsync();

            if (material == null)
                return NotFound();

            return Ok(material);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<StudyMaterialDto>> CreateStudyMaterial(
            [FromForm] CreateStudyMaterialDto dto)
        {
            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.TopicID == dto.TopicID);

            if (topic == null)
                return NotFound("Topic not found.");

            var title = dto.Title.Trim();
            var resourceType = dto.ResourceType.Trim();

            var hasFile = dto.File != null;
            var hasUrl = !string.IsNullOrWhiteSpace(dto.FileURL);

            if (!hasFile && !hasUrl)
                return BadRequest(
                    "A study material must have either a file or a URL.");

            if (hasFile && hasUrl)
                return BadRequest(
                    "A study material cannot have both a file and a URL.");

            var material = new StudyMaterial
            {
                TopicID = dto.TopicID,
                Topic = topic,
                Title = title,
                Description = dto.Description?.Trim(),
                ResourceType = resourceType,
                FileURL = dto.FileURL?.Trim()
            };

            if (dto.File != null)
            {
                using var memoryStream = new MemoryStream();

                await dto.File.CopyToAsync(memoryStream);

                material.FileData = memoryStream.ToArray();
                material.FileName = dto.File.FileName;
                material.FileContentType = dto.File.ContentType;
            }

            _context.StudyMaterials.Add(material);
            await _context.SaveChangesAsync();

            var result = new StudyMaterialDto
            {
                StudyMaterialID = material.StudyMaterialID,
                TopicID = topic.TopicID,
                TopicName = topic.TopicName,
                SubjectID = topic.SubjectID,
                SubjectName = topic.Subject.SubjectName,
                GradeLevel = topic.GradeLevel,
                Title = material.Title,
                Description = material.Description,
                ResourceType = material.ResourceType,
                FileName = material.FileName,
                FileURL = material.FileURL
            };

            return CreatedAtAction(
                nameof(GetStudyMaterial),
                new { id = material.StudyMaterialID },
                result
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateStudyMaterial(
            int id,
            [FromForm] UpdateStudyMaterialDto dto)
        {
            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(sm => sm.StudyMaterialID == id);

            if (material == null)
                return NotFound();

            var topic = await _context.Topics
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.TopicID == dto.TopicID);

            if (topic == null)
                return NotFound("Topic not found.");

            var hasNewFile = dto.File != null;
            var hasNewUrl = !string.IsNullOrWhiteSpace(dto.FileURL);

            if (hasNewFile && hasNewUrl)
                return BadRequest(
                    "A study material cannot have both a file and a URL.");

            material.TopicID = dto.TopicID;
            material.Topic = topic;
            material.Title = dto.Title.Trim();
            material.Description = dto.Description?.Trim();
            material.ResourceType = dto.ResourceType.Trim();

            if (hasNewFile)
            {
                using var memoryStream = new MemoryStream();

                await dto.File!.CopyToAsync(memoryStream);

                material.FileData = memoryStream.ToArray();
                material.FileName = dto.File.FileName;
                material.FileContentType = dto.File.ContentType;
                material.FileURL = null;
            }
            else if (hasNewUrl)
            {
                material.FileData = null;
                material.FileName = null;
                material.FileContentType = null;
                material.FileURL = dto.FileURL!.Trim();
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyMaterial(int id)
        {
            var material = await _context.StudyMaterials
                .FindAsync(id);

            if (material == null)
                return NotFound();

            _context.StudyMaterials.Remove(material);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/file")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var material = await _context.StudyMaterials
                .FindAsync(id);

            if (material == null)
                return NotFound();

            if (material.FileData == null)
                return NotFound(
                    "This study material does not contain an uploaded file.");

            return File(
                material.FileData,
                material.FileContentType ?? "application/octet-stream",
                material.FileName ?? "download"
            );
        }
    }
}