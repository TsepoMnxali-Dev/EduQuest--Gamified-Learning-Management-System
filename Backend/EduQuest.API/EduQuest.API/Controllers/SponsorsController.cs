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
    public class SponsorsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public SponsorsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/sponsors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorDto>>> GetSponsors()
        {
            var sponsors = await _context.Sponsors
                .Select(sponsor => new SponsorDto
                {
                    SponsorID = sponsor.SponsorID,
                    CompanyName = sponsor.CompanyName,
                    ContactEmail = sponsor.ContactEmail
                })
                .ToListAsync();

            return Ok(sponsors);
        }

        // GET: api/sponsors/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorDto>> GetSponsor(int id)
        {
            var sponsor = await _context.Sponsors
                .Where(sponsor => sponsor.SponsorID == id)
                .Select(sponsor => new SponsorDto
                {
                    SponsorID = sponsor.SponsorID,
                    CompanyName = sponsor.CompanyName,
                    ContactEmail = sponsor.ContactEmail
                })
                .FirstOrDefaultAsync();

            if (sponsor == null)
            {
                return NotFound();
            }

            return Ok(sponsor);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<SponsorDto>> CreateSponsor(CreateSponsorDto dto)
        {
            // Normalize the input
            var companyName = dto.CompanyName.Trim();
            var contactEmail = dto.ContactEmail.Trim();

            // Layer 2: Prevent duplicate sponsors by contact email
            var exists = await _context.Sponsors
                .AnyAsync(sponsor => sponsor.ContactEmail == contactEmail);

            if (exists)
            {
                return Conflict("A sponsor with this contact email already exists.");
            }

            var sponsorEntity = new Sponsor
            {
                CompanyName = companyName,
                ContactEmail = contactEmail
            };

            _context.Sponsors.Add(sponsorEntity);
            await _context.SaveChangesAsync();

            var result = new SponsorDto
            {
                SponsorID = sponsorEntity.SponsorID,
                CompanyName = sponsorEntity.CompanyName,
                ContactEmail = sponsorEntity.ContactEmail
            };

            return CreatedAtAction(
                nameof(GetSponsor),
                new { id = sponsorEntity.SponsorID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSponsor(
            int id,
            UpdateSponsorDto dto)
        {
            var sponsorEntity = await _context.Sponsors.FindAsync(id);

            if (sponsorEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var companyName = dto.CompanyName.Trim();
            var contactEmail = dto.ContactEmail.Trim();

            // Layer 2: Prevent duplicate sponsors by contact email
            // Exclude the current sponsor from the check.
            var exists = await _context.Sponsors
                .AnyAsync(sponsor =>
                    sponsor.ContactEmail == contactEmail &&
                    sponsor.SponsorID != id);

            if (exists)
            {
                return Conflict("A sponsor with this contact email already exists.");
            }

            sponsorEntity.CompanyName = companyName;
            sponsorEntity.ContactEmail = contactEmail;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSponsor(int id)
        {
            var sponsorEntity = await _context.Sponsors.FindAsync(id);

            if (sponsorEntity == null)
            {
                return NotFound();
            }

            _context.Sponsors.Remove(sponsorEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}