using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/learners/{learnerId}/subjects")]
    [ApiController]
    public class LearnerSubjectsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LearnerSubjectsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/learners/{learnerId}/subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearnerSubjectDto>>> GetLearnerSubjects(int learnerId)
        {
            var learnerExists = await _context.Learners
                .AnyAsync(l => l.LearnerID == learnerId);

            if (!learnerExists)
            {
                return NotFound();
            }

            var subjects = await _context.learnerSubjects
                .Where(ls => ls.LearnerID == learnerId)
                .Select(ls => new LearnerSubjectDto
                {
                    SubjectID = ls.SubjectID,
                    SubjectName = ls.Subject.SubjectName,
                    GradeLevel = ls.GradeLevel
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpPost("{subjectId}")]
        public async Task<ActionResult<LearnerSubjectDto>> AddSubjectToLearner(
    int learnerId,
    int subjectId,
    CreateLearnerSubjectDto dto)
        {
            var learner = await _context.Learners.FindAsync(learnerId);

            if (learner == null)
            {
                return NotFound("Learner not found.");
            }

            var subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var alreadyExists = await _context.learnerSubjects
                .AnyAsync(ls =>
                    ls.LearnerID == learnerId &&
                    ls.SubjectID == subjectId);

            if (alreadyExists)
            {
                return Conflict("This subject is already assigned to this learner.");
            }

            var learnerSubject = new LearnerSubject
            {
                LearnerID = learnerId,
                Learner = learner,
                SubjectID = subjectId,
                Subject = subject,
                GradeLevel = dto.GradeLevel
            };

            _context.learnerSubjects.Add(learnerSubject);
            await _context.SaveChangesAsync();

            var result = new LearnerSubjectDto
            {
                SubjectID = subject.SubjectID,
                SubjectName = subject.SubjectName,
                GradeLevel = learnerSubject.GradeLevel
            };

            return Ok(result);
        }

        [HttpDelete("{subjectId}")]
        public async Task<IActionResult> RemoveSubjectFromLearner(
    int learnerId,
    int subjectId)
        {
            var learnerSubject = await _context.learnerSubjects
                .FirstOrDefaultAsync(ls =>
                    ls.LearnerID == learnerId &&
                    ls.SubjectID == subjectId);

            if (learnerSubject == null)
            {
                return NotFound("This subject is not assigned to this learner.");
            }

            _context.learnerSubjects.Remove(learnerSubject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}