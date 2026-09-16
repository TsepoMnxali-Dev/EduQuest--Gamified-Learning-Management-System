using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/learners/{learnerId}/achievements")]
    [ApiController]
    public class LearnerAchievementsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LearnerAchievementsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/learners/{learnerId}/achievements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearnerAchievementDto>>> GetLearnerAchievements(int learnerId)
        {
            // Layer 2: Make sure the learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == learnerId);

            if (!learnerExists)
            {
                return NotFound("The specified learner does not exist.");
            }

            var achievements = await _context.LearnerAchievements
                .Where(la => la.LearnerID == learnerId)
                .Select(la => new LearnerAchievementDto
                {
                    LearnerAchievementID = la.LearnerAchievementID,
                    LearnerID = la.LearnerID,
                    AchievementID = la.AchievementID
                })
                .ToListAsync();

            return Ok(achievements);
        }


        [Authorize(Roles = "Admin")]

        // POST: api/learners/{learnerId}/achievements
        [HttpPost]
        public async Task<ActionResult<LearnerAchievementDto>> CreateLearnerAchievement(
            int learnerId,
            CreateLearnerAchievementDto dto)
        {
            // Layer 2: Make sure the learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == learnerId);

            if (!learnerExists)
            {
                return NotFound("The specified learner does not exist.");
            }

            // Layer 2: Make sure the achievement actually exists
            var achievementExists = await _context.Achievements
                .AnyAsync(achievement => achievement.AchievementID == dto.AchievementID);

            if (!achievementExists)
            {
                return BadRequest("The specified AchievementID does not exist.");
            }

            // Layer 2: Prevent awarding the same achievement to the same learner twice
            var alreadyEarned = await _context.LearnerAchievements
                .AnyAsync(la =>
                    la.LearnerID == learnerId &&
                    la.AchievementID == dto.AchievementID);

            if (alreadyEarned)
            {
                return Conflict("This learner has already earned this achievement.");
            }

            // NOTE: This does not check the learner's points against
            // Achievement.PointsRequired. If achievements should only be
            // awarded once a learner has earned enough points, that check
            // (e.g. against the learner's LeaderBoard.TotalPoints) would
            // belong here.

            var learnerAchievementEntity = new LearnerAchievement
            {
                LearnerID = learnerId,
                AchievementID = dto.AchievementID
            };

            _context.LearnerAchievements.Add(learnerAchievementEntity);
            await _context.SaveChangesAsync();

            var result = new LearnerAchievementDto
            {
                LearnerAchievementID = learnerAchievementEntity.LearnerAchievementID,
                LearnerID = learnerAchievementEntity.LearnerID,
                AchievementID = learnerAchievementEntity.AchievementID
            };

            return CreatedAtAction(
                nameof(GetLearnerAchievements),
                new { learnerId = learnerAchievementEntity.LearnerID },
                result
            );
        }
    }
}