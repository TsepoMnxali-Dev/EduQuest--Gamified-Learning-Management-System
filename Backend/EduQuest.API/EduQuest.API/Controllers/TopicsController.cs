using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

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

        [HttpPost]
        public async Task<ActionResult<TopicDto>> CreateTopic(CreateTopicDto dto)
        {
            var subject = await _context.Subjects
                .FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var topic = new Topic
            {
                SubjectID = dto.SubjectID,
                Subject = subject,
                TopicName = dto.TopicName,
                GradeLevel = dto.GradeLevel
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(int id, UpdateTopicDto dto)
        {
            var topic = await _context.Topics.FindAsync(id);

            if (topic == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.FindAsync(dto.SubjectID);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            topic.SubjectID = dto.SubjectID;
            topic.Subject = subject;
            topic.TopicName = dto.TopicName;
            topic.GradeLevel = dto.GradeLevel;

            await _context.SaveChangesAsync();

            return NoContent();
        }

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