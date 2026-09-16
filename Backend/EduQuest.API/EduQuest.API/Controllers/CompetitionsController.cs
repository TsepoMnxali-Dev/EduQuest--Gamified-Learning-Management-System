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
    public class CompetitionsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CompetitionsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/competitions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompetitionDto>>> GetCompetitions()
        {
            var competitions = await _context.Competitions
                .Select(competition => new CompetitionDto
                {
                    CompetitionID = competition.CompetitionID,
                    SponsorName = competition.SponsorName,
                    StartDate = competition.StartDate,
                    EndDate = competition.EndDate,
                    Description = competition.Description,
                    SponsorID = competition.SponsorID
                })
                .ToListAsync();

            return Ok(competitions);
        }

        // GET: api/competitions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CompetitionDto>> GetCompetition(int id)
        {
            var competition = await _context.Competitions
                .Where(competition => competition.CompetitionID == id)
                .Select(competition => new CompetitionDto
                {
                    CompetitionID = competition.CompetitionID,
                    SponsorName = competition.SponsorName,
                    StartDate = competition.StartDate,
                    EndDate = competition.EndDate,
                    Description = competition.Description,
                    SponsorID = competition.SponsorID
                })
                .FirstOrDefaultAsync();

            if (competition == null)
            {
                return NotFound();
            }

            return Ok(competition);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<CompetitionDto>> CreateCompetition(CreateCompetitionDto dto)
        {
            // Normalize the input
            var sponsorName = dto.SponsorName?.Trim();
            var startDateRaw = dto.StartDate.Trim();
            var endDateRaw = dto.EndDate.Trim();
            var description = dto.Description.Trim();

            // Layer 2: Check that both dates actually parse
            if (!DateTime.TryParse(startDateRaw, out var startDate))
            {
                return BadRequest("StartDate is not a valid date.");
            }

            if (!DateTime.TryParse(endDateRaw, out var endDate))
            {
                return BadRequest("EndDate is not a valid date.");
            }

            // Layer 2: EndDate must be after StartDate
            if (endDate <= startDate)
            {
                return BadRequest("EndDate must be after StartDate.");
            }

            // Layer 2: Check that the referenced Sponsor actually exists
            var sponsorExists = await _context.Sponsors
                .AnyAsync(sponsor => sponsor.SponsorID == dto.SponsorID);

            if (!sponsorExists)
            {
                return BadRequest("The specified SponsorID does not exist.");
            }

            var competitionEntity = new Competition
            {
                SponsorName = sponsorName,
                StartDate = startDateRaw,
                EndDate = endDateRaw,
                Description = description,
                SponsorID = dto.SponsorID
            };

            _context.Competitions.Add(competitionEntity);
            await _context.SaveChangesAsync();

            var result = new CompetitionDto
            {
                CompetitionID = competitionEntity.CompetitionID,
                SponsorName = competitionEntity.SponsorName,
                StartDate = competitionEntity.StartDate,
                EndDate = competitionEntity.EndDate,
                Description = competitionEntity.Description,
                SponsorID = competitionEntity.SponsorID
            };

            return CreatedAtAction(
                nameof(GetCompetition),
                new { id = competitionEntity.CompetitionID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompetition(
            int id,
            UpdateCompetitionDto dto)
        {
            var competitionEntity = await _context.Competitions.FindAsync(id);

            if (competitionEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var sponsorName = dto.SponsorName?.Trim();
            var startDateRaw = dto.StartDate.Trim();
            var endDateRaw = dto.EndDate.Trim();
            var description = dto.Description.Trim();

            // Layer 2: Check that both dates actually parse
            if (!DateTime.TryParse(startDateRaw, out var startDate))
            {
                return BadRequest("StartDate is not a valid date.");
            }

            if (!DateTime.TryParse(endDateRaw, out var endDate))
            {
                return BadRequest("EndDate is not a valid date.");
            }

            // Layer 2: EndDate must be after StartDate
            if (endDate <= startDate)
            {
                return BadRequest("EndDate must be after StartDate.");
            }

            // Layer 2: Check that the referenced Sponsor actually exists
            var sponsorExists = await _context.Sponsors
                .AnyAsync(sponsor => sponsor.SponsorID == dto.SponsorID);

            if (!sponsorExists)
            {
                return BadRequest("The specified SponsorID does not exist.");
            }

            competitionEntity.SponsorName = sponsorName;
            competitionEntity.StartDate = startDateRaw;
            competitionEntity.EndDate = endDateRaw;
            competitionEntity.Description = description;
            competitionEntity.SponsorID = dto.SponsorID;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompetition(int id)
        {
            var competitionEntity = await _context.Competitions.FindAsync(id);

            if (competitionEntity == null)
            {
                return NotFound();
            }

            _context.Competitions.Remove(competitionEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}