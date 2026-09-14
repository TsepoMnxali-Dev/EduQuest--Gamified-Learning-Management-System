using EduQuest.API.Data;
using EduQuest.API.DTOs.Schools;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public SchoolsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/schools
        // Optional filter: api/schools?provinceId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchoolDto>>> GetSchools(
            int? provinceId = null)
        {
            if (provinceId.HasValue)
            {
                var provinceExists = await _context.Provinces
                    .AnyAsync(p => p.ProvinceID == provinceId.Value);

                if (!provinceExists)
                    return NotFound("Province not found.");
            }

            var schools = await _context.Schools
                .Where(s => !provinceId.HasValue ||
                            s.ProvinceID == provinceId.Value)
                .Select(s => new SchoolDto
                {
                    SchoolID = s.SchoolID,
                    SchoolName = s.SchoolName,
                    ProvinceID = s.ProvinceID,
                    ProvinceName = s.Province.ProvinceName
                })
                .ToListAsync();

            return Ok(schools);
        }

        // GET: api/schools/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolDto>> GetSchool(int id)
        {
            var school = await _context.Schools
                .Where(s => s.SchoolID == id)
                .Select(s => new SchoolDto
                {
                    SchoolID = s.SchoolID,
                    SchoolName = s.SchoolName,
                    ProvinceID = s.ProvinceID,
                    ProvinceName = s.Province.ProvinceName
                })
                .FirstOrDefaultAsync();

            if (school == null)
                return NotFound();

            return Ok(school);
        }

        // POST: api/schools
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SchoolDto>> CreateSchool(
            CreateSchoolDto dto)
        {
            var schoolName = dto.SchoolName.Trim();

            var province = await _context.Provinces
                .FindAsync(dto.ProvinceID);

            if (province == null)
                return NotFound("Province not found.");

            var alreadyExists = await _context.Schools
                .AnyAsync(s =>
                    s.ProvinceID == dto.ProvinceID &&
                    s.SchoolName == schoolName);

            if (alreadyExists)
                return Conflict(
                    "A school with this name already exists in this province.");

            var school = new School
            {
                SchoolName = schoolName,
                ProvinceID = dto.ProvinceID
            };

            _context.Schools.Add(school);
            await _context.SaveChangesAsync();

            var result = new SchoolDto
            {
                SchoolID = school.SchoolID,
                SchoolName = school.SchoolName,
                ProvinceID = school.ProvinceID,
                ProvinceName = province.ProvinceName
            };

            return CreatedAtAction(
                nameof(GetSchool),
                new { id = school.SchoolID },
                result);
        }

        // PUT: api/schools/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchool(
            int id,
            UpdateSchoolDto dto)
        {
            var school = await _context.Schools
                .FindAsync(id);

            if (school == null)
                return NotFound();

            var province = await _context.Provinces
                .FindAsync(dto.ProvinceID);

            if (province == null)
                return NotFound("Province not found.");

            var schoolName = dto.SchoolName.Trim();

            var alreadyExists = await _context.Schools
                .AnyAsync(s =>
                    s.ProvinceID == dto.ProvinceID &&
                    s.SchoolName == schoolName &&
                    s.SchoolID != id);

            if (alreadyExists)
                return Conflict(
                    "A school with this name already exists in this province.");

            school.SchoolName = schoolName;
            school.ProvinceID = dto.ProvinceID;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/schools/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchool(int id)
        {
            var school = await _context.Schools
                .FindAsync(id);

            if (school == null)
                return NotFound();

            var hasLearners = await _context.Learners
                .AnyAsync(l => l.SchoolID == id);

            if (hasLearners)
            {
                return Conflict(
                    "This school cannot be deleted because learners are assigned to it.");
            }

            _context.Schools.Remove(school);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}