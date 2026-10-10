
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

        // NEW: Maximum file size is 10 MB.
        private const long MaxFileSize = 10 * 1024 * 1024;

        // NEW: Only these document extensions are accepted.
        private static readonly string[] AllowedExtensions =
        {
            ".pdf", ".doc", ".docx",
            ".ppt", ".pptx", ".txt"
        };

        public StudyMaterialsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // ==============================================
        // GET ALL STUDY MATERIALS
        // Admins and authenticated learners can access.
        // ==============================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyMaterialDto>>>
            GetStudyMaterials()
        {
            var materials = await _context.StudyMaterials
                .AsNoTracking()
                .Include(sm => sm.GradeSubject)
                    .ThenInclude(gs => gs.Subject)
                .Include(sm => sm.GradeSubject)
                    .ThenInclude(gs => gs.Grade)
                .Include(sm => sm.Topic)
                .Select(sm => new StudyMaterialDto
                {
                    StudyMaterialID = sm.StudyMaterialID,
                    GradeSubjectID = sm.GradeSubjectID,
                    TopicID = sm.TopicID,

                    TopicName = sm.Topic != null
                        ? sm.Topic.TopicName
                        : null,

                    SubjectID = sm.GradeSubject.SubjectID,
                    SubjectName = sm.GradeSubject.Subject.SubjectName,
                    GradeLevel = sm.GradeSubject.Grade.GradeName,

                    Title = sm.Title,
                    Description = sm.Description,
                    ResourceType = sm.ResourceType,
                    FileName = sm.FileName,
                    FileURL = sm.FileURL
                })
                .ToListAsync();

            // NEW: An empty database returns [].
            // The frontend can display "No resources yet".
            return Ok(materials);
        }

        // ==============================================
        // GET ONE STUDY MATERIAL
        // ==============================================

        [HttpGet("{id}")]
        public async Task<ActionResult<StudyMaterialDto>>
            GetStudyMaterial(int id)
        {
            var material = await _context.StudyMaterials
                .AsNoTracking()
                .Where(sm => sm.StudyMaterialID == id)
                .Select(sm => new StudyMaterialDto
                {
                    StudyMaterialID = sm.StudyMaterialID,
                    GradeSubjectID = sm.GradeSubjectID,
                    TopicID = sm.TopicID,

                    TopicName = sm.Topic != null
                        ? sm.Topic.TopicName
                        : null,

                    SubjectID = sm.GradeSubject.SubjectID,
                    SubjectName = sm.GradeSubject.Subject.SubjectName,
                    GradeLevel = sm.GradeSubject.Grade.GradeName,

                    Title = sm.Title,
                    Description = sm.Description,
                    ResourceType = sm.ResourceType,
                    FileName = sm.FileName,
                    FileURL = sm.FileURL
                })
                .FirstOrDefaultAsync();

            if (material == null)
                return NotFound("Study material not found.");

            return Ok(material);
        }

        // ==============================================
        // CREATE STUDY MATERIAL
        // SECURITY: ONLY ADMIN CAN UPLOAD OR ADD LINKS
        // ==============================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<StudyMaterialDto>>
            CreateStudyMaterial([FromForm] CreateStudyMaterialDto dto)
        {
            // Validate grade and subject combination.
            var gradeSubject = await _context.GradeSubjects
                .Include(gs => gs.Grade)
                .Include(gs => gs.Subject)
                .FirstOrDefaultAsync(gs =>
                    gs.GradeSubjectID == dto.GradeSubjectID);

            if (gradeSubject == null)
                return NotFound("Grade and subject combination not found.");

            Topic? topic = null;

            if (dto.TopicID.HasValue)
            {
                topic = await _context.Topics
                    .FirstOrDefaultAsync(t =>
                        t.TopicID == dto.TopicID.Value);

                if (topic == null)
                    return NotFound("Topic not found.");

                // SECURITY: Prevent assigning material
                // to an unrelated grade or subject.
                if (topic.SubjectID != gradeSubject.SubjectID ||
                    topic.GradeLevel != gradeSubject.Grade.GradeName)
                {
                    return BadRequest(
                        "Topic does not belong to the selected grade and subject.");
                }
            }

            bool hasFile = dto.File != null;
            bool hasUrl = !string.IsNullOrWhiteSpace(dto.FileURL);

            // NEW: A resource must contain exactly one
            // source: either a document or a link.
            if (hasFile == hasUrl)
            {
                return BadRequest(
                    "Provide either one document or one URL.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.ResourceType))
            {
                return BadRequest(
                    "Title and resource type are required.");
            }

            // NEW: Check that resource links use HTTP/HTTPS.
            if (hasUrl && !IsValidResourceUrl(dto.FileURL))
            {
                return BadRequest(
                    "Resource URL must be a valid HTTP or HTTPS link.");
            }

            var material = new StudyMaterial
            {
                GradeSubjectID = dto.GradeSubjectID,
                GradeSubject = gradeSubject,

                TopicID = dto.TopicID,
                Topic = topic,

                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                ResourceType = dto.ResourceType.Trim(),

                FileURL = hasUrl ? dto.FileURL!.Trim() : null
            };

            if (hasFile)
            {
                // NEW: Validate file size and extension.
                var error = ValidateFile(dto.File!);

                if (error != null)
                    return BadRequest(error);

                using var stream = new MemoryStream();
                await dto.File!.CopyToAsync(stream);

                // Preserve existing database storage design.
                material.FileData = stream.ToArray();

                // SECURITY: Remove any path from the filename.
                material.FileName = Path.GetFileName(dto.File.FileName);
                material.FileContentType = GetContentType(
                    Path.GetExtension(material.FileName)
                );
            }

            _context.StudyMaterials.Add(material);
            await _context.SaveChangesAsync();

            var result = new StudyMaterialDto
            {
                StudyMaterialID = material.StudyMaterialID,
                GradeSubjectID = gradeSubject.GradeSubjectID,

                TopicID = topic?.TopicID,
                TopicName = topic?.TopicName,

                SubjectID = gradeSubject.SubjectID,
                SubjectName = gradeSubject.Subject.SubjectName,
                GradeLevel = gradeSubject.Grade.GradeName,

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

        // ==============================================
        // UPDATE STUDY MATERIAL
        // SECURITY: ONLY ADMIN
        // ==============================================

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateStudyMaterial(
            int id,
            [FromForm] UpdateStudyMaterialDto dto)
        {
            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(sm =>
                    sm.StudyMaterialID == id);

            if (material == null)
                return NotFound("Study material not found.");

            var gradeSubject = await _context.GradeSubjects
                .Include(gs => gs.Grade)
                .Include(gs => gs.Subject)
                .FirstOrDefaultAsync(gs =>
                    gs.GradeSubjectID == dto.GradeSubjectID);

            if (gradeSubject == null)
                return NotFound("Grade and subject combination not found.");

            Topic? topic = null;

            if (dto.TopicID.HasValue)
            {
                topic = await _context.Topics
                    .FirstOrDefaultAsync(t =>
                        t.TopicID == dto.TopicID.Value);

                if (topic == null)
                    return NotFound("Topic not found.");

                if (topic.SubjectID != gradeSubject.SubjectID ||
                    topic.GradeLevel != gradeSubject.Grade.GradeName)
                {
                    return BadRequest(
                        "Topic does not belong to the selected grade and subject.");
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.ResourceType))
            {
                return BadRequest(
                    "Title and resource type are required.");
            }

            bool hasNewFile = dto.File != null;
            bool hasNewUrl = !string.IsNullOrWhiteSpace(dto.FileURL);

            if (hasNewFile && hasNewUrl)
            {
                return BadRequest(
                    "Provide a document or URL, not both.");
            }

            if (hasNewUrl && !IsValidResourceUrl(dto.FileURL))
            {
                return BadRequest(
                    "Resource URL must be a valid HTTP or HTTPS link.");
            }

            // NEW: Validate before changing the stored resource.
            if (hasNewFile)
            {
                var error = ValidateFile(dto.File!);

                if (error != null)
                    return BadRequest(error);
            }

            material.GradeSubjectID = dto.GradeSubjectID;
            material.GradeSubject = gradeSubject;

            material.TopicID = dto.TopicID;
            material.Topic = topic;

            material.Title = dto.Title.Trim();
            material.Description = dto.Description?.Trim();
            material.ResourceType = dto.ResourceType.Trim();

            if (hasNewFile)
            {
                using var stream = new MemoryStream();
                await dto.File!.CopyToAsync(stream);

                // CHANGED: Replace previous file or link.
                material.FileData = stream.ToArray();
                material.FileName = Path.GetFileName(dto.File.FileName);
                material.FileContentType = GetContentType(
                    Path.GetExtension(material.FileName)
                );
                material.FileURL = null;
            }
            else if (hasNewUrl)
            {
                // CHANGED: Replace previous file with a link.
                material.FileData = null;
                material.FileName = null;
                material.FileContentType = null;
                material.FileURL = dto.FileURL!.Trim();
            }

            // When no new file or URL is supplied,
            // preserve the existing resource source.
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==============================================
        // DELETE STUDY MATERIAL
        // SECURITY: ONLY ADMIN
        // ==============================================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyMaterial(int id)
        {
            var material = await _context.StudyMaterials
                .FindAsync(id);

            if (material == null)
                return NotFound("Study material not found.");

            _context.StudyMaterials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==============================================
        // DOWNLOAD STUDY MATERIAL
        // Admins and authenticated learners can download.
        // ==============================================

        [HttpGet("{id}/file")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var material = await _context.StudyMaterials
                .AsNoTracking()
                .FirstOrDefaultAsync(sm =>
                    sm.StudyMaterialID == id);

            if (material == null)
                return NotFound("Study material not found.");

            if (material.FileData == null)
            {
                return NotFound(
                    "This resource is a link or has no uploaded document.");
            }

            return File(
                material.FileData,
                material.FileContentType ?? "application/octet-stream",
                material.FileName ?? "download"
            );
        }

        // ==============================================
        // NEW: HELPER METHODS
        // ==============================================

        private static string? ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                return "The uploaded document is empty.";

            if (file.Length > MaxFileSize)
                return "Maximum allowed file size is 10 MB.";

            string extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return "Unsupported file type.";

            return null;
        }

        private static bool IsValidResourceUrl(string? url)
        {
            return Uri.TryCreate(
                url?.Trim(),
                UriKind.Absolute,
                out Uri? result
            ) &&
            (result.Scheme == Uri.UriSchemeHttp ||
             result.Scheme == Uri.UriSchemeHttps);
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}

