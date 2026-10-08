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
    public class PrizesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public PrizesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/prizes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrizeDto>>> GetPrizes()
        {
            var prizes = await _context.Prizes
                .Select(prize => new PrizeDto
                {
                    PrizeID = prize.PrizeID,
                    PrizeName = prize.PrizeName,
                    PrizeDescription = prize.PrizeDescription,
                    Value = prize.Value,
                    CompetitionID = prize.CompetitionID
                })
                .ToListAsync();

            return Ok(prizes);
        }

        // GET: api/prizes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PrizeDto>> GetPrize(int id)
        {
            var prize = await _context.Prizes
                .Where(prize => prize.PrizeID == id)
                .Select(prize => new PrizeDto
                {
                    PrizeID = prize.PrizeID,
                    PrizeName = prize.PrizeName,
                    PrizeDescription = prize.PrizeDescription,
                    Value = prize.Value,
                    CompetitionID = prize.CompetitionID
                })
                .FirstOrDefaultAsync();

            if (prize == null)
            {
                return NotFound();
            }

            return Ok(prize);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<PrizeDto>> CreatePrize(CreatePrizeDto dto)
        {
            // Normalize the input
            var prizeName = dto.PrizeName.Trim();
            var prizeDescription = dto.PrizeDescription.Trim();

            // Layer 2: Check that the referenced Competition actually exists
            var competitionExists = await _context.Competitions
                .AnyAsync(competition => competition.CompetitionID == dto.CompetitionID);

            if (!competitionExists)
            {
                return BadRequest("The specified CompetitionID does not exist.");
            }

            var prizeEntity = new Prize
            {
                PrizeName = prizeName,
                PrizeDescription = prizeDescription,
                Value = dto.Value,
                CompetitionID = dto.CompetitionID
            };

            _context.Prizes.Add(prizeEntity);
            await _context.SaveChangesAsync();

            var result = new PrizeDto
            {
                PrizeID = prizeEntity.PrizeID,
                PrizeName = prizeEntity.PrizeName,
                PrizeDescription = prizeEntity.PrizeDescription,
                Value = prizeEntity.Value,
                CompetitionID = prizeEntity.CompetitionID
            };

            return CreatedAtAction(
                nameof(GetPrize),
                new { id = prizeEntity.PrizeID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrize(
            int id,
            UpdatePrizeDto dto)
        {
            var prizeEntity = await _context.Prizes.FindAsync(id);

            if (prizeEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var prizeName = dto.PrizeName.Trim();
            var prizeDescription = dto.PrizeDescription.Trim();

            // Layer 2: Check that the referenced Competition actually exists
            var competitionExists = await _context.Competitions
                .AnyAsync(competition => competition.CompetitionID == dto.CompetitionID);

            if (!competitionExists)
            {
                return BadRequest("The specified CompetitionID does not exist.");
            }

            prizeEntity.PrizeName = prizeName;
            prizeEntity.PrizeDescription = prizeDescription;
            prizeEntity.Value = dto.Value;
            prizeEntity.CompetitionID = dto.CompetitionID;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrize(int id)
        {
            var prizeEntity = await _context.Prizes.FindAsync(id);

            if (prizeEntity == null)
            {
                return NotFound();
            }

            _context.Prizes.Remove(prizeEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}