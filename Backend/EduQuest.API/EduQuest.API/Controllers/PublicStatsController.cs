//using EduQuest.API.Data;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace EduQuest.API.Controllers
//{
//    // Public numbers for the landing page. Contains counts only,
//    // no personal information, so it is safe to leave anonymous.
//    [AllowAnonymous]
//    [ApiController]
//    [Route("api/public-stats")]
//    public class PublicStatsController : ControllerBase
//    {
//        private readonly ApplicationDBContext _context;

//        public PublicStatsController(ApplicationDBContext context)
//        {
//            _context = context;
//        }

//        // GET: api/public-stats
//        [HttpGet]
//        [ResponseCache(Duration = 60)]
//        public async Task<IActionResult> GetPublicStats()
//        {
//            var studentsEnrolled = await _context.Learners.CountAsync();

//            var schoolsReached = await _context.Learners
//                .Select(l => l.SchoolID)
//                .Distinct()
//                .CountAsync();

//            // Stays at 0 until quiz results are connected below.
//            int improvedPercent = 0;

//            return Ok(new
//            {
//                studentsEnrolled,
//                schoolsReached,
//                improvedPercent
//            });
//        }
//    }
//}