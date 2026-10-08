using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/competitions/{competitionId}/participants")]
    [ApiController]
    public class CompetitionLearnersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CompetitionLearnersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/competitions/{competitionId}/participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompetitionLearnerDto>>> GetParticipants(int competitionId)
        {
            // Layer 2: Make sure the competition actually exists
            var competitionExists = await _context.Competitions
                .AnyAsync(competition => competition.CompetitionID == competitionId);

            if (!competitionExists)
            {
                return NotFound("The specified competition does not exist.");
            }

            var participants = await _context.CompetitionLearners
                .Where(cl => cl.CompetitionID == competitionId)
                .Select(cl => new CompetitionLearnerDto
                {
                    CompetitionLearnerID = cl.CompetitionLearnerID,
                    CompetitionID = cl.CompetitionID,
                    LearnerID = cl.LearnerID,
                    Score = cl.Score,
                    Position = cl.Position
                })
                .ToListAsync();

            return Ok(participants);
        }

        // POST: api/competitions/{competitionId}/participants
        // No [Authorize(Roles = "Admin")] — a learner registers themselves.
        [HttpPost]
        public async Task<ActionResult<CompetitionLearnerDto>> RegisterParticipant(
            int competitionId,
            CreateCompetitionLearnerDto dto)
        {
            // Layer 2: Make sure the competition actually exists
            var competitionExists = await _context.Competitions
                .AnyAsync(competition => competition.CompetitionID == competitionId);

            if (!competitionExists)
            {
                return NotFound("The specified competition does not exist.");
            }

            // Layer 2: Make sure the learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == dto.LearnerID);

            if (!learnerExists)
            {
                return BadRequest("The specified LearnerID does not exist.");
            }

            // Layer 2: Prevent registering the same learner twice for the same competition
            var alreadyRegistered = await _context.CompetitionLearners
                .AnyAsync(cl =>
                    cl.CompetitionID == competitionId &&
                    cl.LearnerID == dto.LearnerID);

            if (alreadyRegistered)
            {
                return Conflict("This learner is already registered for this competition.");
            }

            var competitionLearnerEntity = new CompetitionLearner
            {
                CompetitionID = competitionId,
                LearnerID = dto.LearnerID,
                Score = 0,
                Position = 0
            };

            _context.CompetitionLearners.Add(competitionLearnerEntity);
            await _context.SaveChangesAsync();

            var result = new CompetitionLearnerDto
            {
                CompetitionLearnerID = competitionLearnerEntity.CompetitionLearnerID,
                CompetitionID = competitionLearnerEntity.CompetitionID,
                LearnerID = competitionLearnerEntity.LearnerID,
                Score = competitionLearnerEntity.Score,
                Position = competitionLearnerEntity.Position
            };

            return CreatedAtAction(
                nameof(GetParticipants),
                new { competitionId = competitionLearnerEntity.CompetitionID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        // DELETE: api/competitions/{competitionId}/participants/{learnerId}
        [HttpDelete("{learnerId}")]
        public async Task<IActionResult> RemoveParticipant(int competitionId, int learnerId)
        {
            var competitionLearnerEntity = await _context.CompetitionLearners
                .FirstOrDefaultAsync(cl =>
                    cl.CompetitionID == competitionId &&
                    cl.LearnerID == learnerId);

            if (competitionLearnerEntity == null)
            {
                return NotFound();
            }

            _context.CompetitionLearners.Remove(competitionLearnerEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}