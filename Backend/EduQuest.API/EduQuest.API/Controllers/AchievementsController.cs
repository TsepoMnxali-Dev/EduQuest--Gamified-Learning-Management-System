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
    public class AchievementsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AchievementsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/achievements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAchievements()
        {
            var achievements = await _context.Achievements
                .Select(achievement => new AchievementDto
                {
                    AchievementID = achievement.AchievementID,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    PointsRequired = achievement.PointsRequired,
                    BadgeImage = achievement.BadgeImage
                })
                .ToListAsync();

            return Ok(achievements);
        }

        // GET: api/achievements/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AchievementDto>> GetAchievement(int id)
        {
            var achievement = await _context.Achievements
                .Where(achievement => achievement.AchievementID == id)
                .Select(achievement => new AchievementDto
                {
                    AchievementID = achievement.AchievementID,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    PointsRequired = achievement.PointsRequired,
                    BadgeImage = achievement.BadgeImage
                })
                .FirstOrDefaultAsync();

            if (achievement == null)
            {
                return NotFound();
            }

            return Ok(achievement);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<AchievementDto>> CreateAchievement(CreateAchievementDto dto)
        {
            // Normalize the input
            var name = dto.Name.Trim();
            var description = dto.Description.Trim();
            var badgeImage = dto.BadgeImage.Trim();

            // Layer 2: Prevent duplicate achievement names
            var exists = await _context.Achievements
                .AnyAsync(achievement => achievement.Name == name);

            if (exists)
            {
                return Conflict("An achievement with this name already exists.");
            }

            var achievementEntity = new Achievement
            {
                Name = name,
                Description = description,
                PointsRequired = dto.PointsRequired,
                BadgeImage = badgeImage
            };

            _context.Achievements.Add(achievementEntity);
            await _context.SaveChangesAsync();

            var result = new AchievementDto
            {
                AchievementID = achievementEntity.AchievementID,
                Name = achievementEntity.Name,
                Description = achievementEntity.Description,
                PointsRequired = achievementEntity.PointsRequired,
                BadgeImage = achievementEntity.BadgeImage
            };

            return CreatedAtAction(
                nameof(GetAchievement),
                new { id = achievementEntity.AchievementID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAchievement(
            int id,
            UpdateAchievementDto dto)
        {
            var achievementEntity = await _context.Achievements.FindAsync(id);

            if (achievementEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var name = dto.Name.Trim();
            var description = dto.Description.Trim();
            var badgeImage = dto.BadgeImage.Trim();

            // Layer 2: Prevent duplicate achievement names
            // Exclude the current achievement from the check.
            var exists = await _context.Achievements
                .AnyAsync(achievement =>
                    achievement.Name == name &&
                    achievement.AchievementID != id);

            if (exists)
            {
                return Conflict("An achievement with this name already exists.");
            }

            achievementEntity.Name = name;
            achievementEntity.Description = description;
            achievementEntity.PointsRequired = dto.PointsRequired;
            achievementEntity.BadgeImage = badgeImage;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievementEntity = await _context.Achievements.FindAsync(id);

            if (achievementEntity == null)
            {
                return NotFound();
            }

            _context.Achievements.Remove(achievementEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}