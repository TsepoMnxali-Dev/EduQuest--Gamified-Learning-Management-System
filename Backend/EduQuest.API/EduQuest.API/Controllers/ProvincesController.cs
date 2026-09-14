using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProvincesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ProvincesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/provinces
        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .Select(p => new
                {
                    ProvinceID = p.ProvinceID,
                    ProvinceName = p.ProvinceName
                })
                .ToListAsync();

            return Ok(provinces);
        }

        // GET: api/provinces/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProvince(int id)
        {
            var province = await _context.Provinces
                .Where(p => p.ProvinceID == id)
                .Select(p => new
                {
                    ProvinceID = p.ProvinceID,
                    ProvinceName = p.ProvinceName
                })
                .FirstOrDefaultAsync();

            if (province == null)
                return NotFound();

            return Ok(province);
        }
    }
}