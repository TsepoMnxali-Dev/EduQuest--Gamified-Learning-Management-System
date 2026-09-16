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
    public class TopicsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        // EduQuest currently supports Grades 10–12.
        private static readonly HashSet<string> AllowedGrades =
            new(StringComparer.OrdinalIgnoreCase)
            {
        "Grade 10",
        "Grade 11",
        "Grade 12"
            };

        public TopicsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Topics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicDto>>> GetTopics()
        {
            var topics = await _context.Topics
                .Select(t => new TopicDto
                {
                    TopicID = t.TopicID,
                    SubjectID = t.SubjectID,
                    SubjectName = t.Subject.SubjectName,
                    TopicName = t.TopicName,
                    GradeLevel = t.GradeLevel
                })
                .ToListAsync();

            return Ok(topics);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TopicDto>> GetTopic(int id)
        {
            var topic = await _context.Topics
                .Where(t => t.TopicID == id)
                .Select(t => new TopicDto
                {
                    TopicID = t.TopicID,
                    SubjectID = t.SubjectID,
                    SubjectName = t.Subject.SubjectName,
                    TopicName = t.TopicName,
                    GradeLevel = t.GradeLevel
                })
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                return NotFound();
            }

            return Ok(topic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TopicDto>> CreateTopic(CreateTopicDto dto)
        {
            var subject = await _context.Subjects
                .FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Normalize the input
            var topicName = dto.TopicName.Trim();
            var gradeLevel = dto.GradeLevel.Trim();

            // Layer 2: Validate the grade
            var validGrade = AllowedGrades.FirstOrDefault(
                grade => string.Equals(
                    grade,
                    gradeLevel,
                    StringComparison.OrdinalIgnoreCase));

            if (validGrade == null)
            {
                return BadRequest(
                    "GradeLevel must be Grade 10, Grade 11, or Grade 12.");
            }

            // Store the canonical grade name
            gradeLevel = validGrade;

            // Layer 2: Prevent duplicate topics
            var alreadyExists = await _context.Topics
                .AnyAsync(t =>
                    t.SubjectID == dto.SubjectID &&
                    t.GradeLevel == gradeLevel &&
                    t.TopicName == topicName);

            if (alreadyExists)
            {
                return Conflict(
                    "This topic already exists for this subject and grade.");
            }

            var topic = new Topic
            {
                SubjectID = dto.SubjectID,
                TopicName = topicName,
                GradeLevel = gradeLevel
            };

            _context.Topics.Add(topic);

            await _context.SaveChangesAsync();

            var result = new TopicDto
            {
                TopicID = topic.TopicID,
                SubjectID = topic.SubjectID,
                SubjectName = subject.SubjectName,
                TopicName = topic.TopicName,
                GradeLevel = topic.GradeLevel
            };

            return CreatedAtAction(
                nameof(GetTopic),
                new { id = topic.TopicID },
                result
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(
    int id,
    UpdateTopicDto dto)
        {
            var topic = await _context.Topics.FindAsync(id);

            if (topic == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Normalize the input
            var topicName = dto.TopicName.Trim();
            var gradeLevel = dto.GradeLevel.Trim();

            // Layer 2: Validate the grade
            var validGrade = AllowedGrades.FirstOrDefault(
                grade => string.Equals(
                    grade,
                    gradeLevel,
                    StringComparison.OrdinalIgnoreCase));

            if (validGrade == null)
            {
                return BadRequest(
                    "GradeLevel must be Grade 10, Grade 11, or Grade 12.");
            }

            // Store the canonical grade name
            gradeLevel = validGrade;

            // Layer 2: Prevent duplicate topics
            // Exclude the topic currently being updated.
            var alreadyExists = await _context.Topics
                .AnyAsync(t =>
                    t.SubjectID == dto.SubjectID &&
                    t.GradeLevel == gradeLevel &&
                    t.TopicName == topicName &&
                    t.TopicID != id);

            if (alreadyExists)
            {
                return Conflict(
                    "This topic already exists for this subject and grade.");
            }

            topic.SubjectID = dto.SubjectID;
            topic.TopicName = topicName;
            topic.GradeLevel = gradeLevel;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var topic = await _context.Topics.FindAsync(id);

            if (topic == null)
            {
                return NotFound();
            }

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}