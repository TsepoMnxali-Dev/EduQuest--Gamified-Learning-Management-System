using EduQuest.API.DTOs.Statistics;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authorization;
using EduQuest.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ApplicationDBContext _context;

        public StatisticsController(IStatisticsService statisticsService, ApplicationDBContext context)
        {
            _statisticsService = statisticsService;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("overview")]
        public async Task<ActionResult<StatisticsOverviewDto>> GetOverview()
        {
            var result = await _statisticsService.GetOverviewAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("learners/by-province")]
        public async Task<ActionResult<List<LearnerCountByProvinceDto>>> GetLearnersByProvince()
        {
            var result = await _statisticsService.GetLearnersByProvinceAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("learners/by-school")]
        public async Task<ActionResult<List<LearnerCountBySchoolDto>>> GetLearnersBySchool()
        {
            var result = await _statisticsService.GetLearnersBySchoolAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("learners/by-grade")]
        public async Task<ActionResult<List<LearnerCountByGradeDto>>> GetLearnersByGrade()
        {
            var result = await _statisticsService.GetLearnersByGradeAsync();

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("performance/by-province")]
        public async Task<ActionResult<List<AverageScoreByProvinceDto>>> GetAverageScoreByProvince()
        {
            var result = await _statisticsService.GetAverageScoreByProvinceAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("performance/by-school")]
        public async Task<ActionResult<List<AverageScoreBySchoolDto>>> GetAverageScoreBySchool()
        {
            var result = await _statisticsService.GetAverageScoreBySchoolAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("performance/by-grade")]
        public async Task<ActionResult<List<AverageScoreByGradeDto>>> GetAverageScoreByGrade()
        {
            var result = await _statisticsService.GetAverageScoreByGradeAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("performance/by-subject")]
        public async Task<ActionResult<List<AverageScoreBySubjectDto>>> GetAverageScoreBySubject()
        {
            var result = await _statisticsService.GetAverageScoreBySubjectAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("performance/by-topic")]
        public async Task<ActionResult<List<AverageScoreByTopicDto>>> GetAverageScoreByTopic()
        {
            var result = await _statisticsService.GetAverageScoreByTopicAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("performance/pass-fail")]
        public async Task<ActionResult<PassFailStatisticsDto>> GetPassFailStatistics()
        {
            var result = await _statisticsService.GetPassFailStatisticsAsync();

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("engagement/quiz-attempts")]
        public async Task<ActionResult<QuizStatisticsDto>> GetQuizAttemptsStatistics()
        {
            var result = await _statisticsService.GetQuizAttemptsStatisticsAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("engagement/quiz-completion")]
        public async Task<ActionResult<QuizCompletionStatisticsDto>> GetQuizCompletionStatistics()
        {
            var result = await _statisticsService.GetQuizCompletionStatisticsAsync();

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("engagement/competitions")]
        public async Task<ActionResult<CompetitionStatisticsDto>> GetCompetitionStatistics()
        {
            var result = await _statisticsService.GetCompetitionStatisticsAsync();

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("engagement/achievements")]
        public async Task<ActionResult<AchievementStatisticsDto>> GetAchievementStatistics()
        {
            var result = await _statisticsService.GetAchievementStatisticsAsync();

            return Ok(result);
        }
        // Admins can view any learner; a learner can only view their own statistics.
        [HttpGet("learners/{learnerId}")]
        public async Task<ActionResult<IndividualLearnerStatisticsDto>> GetIndividualLearnerStatistics(int learnerId)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Unauthorized();

                var ownsLearner = await _context.Learners
                    .AnyAsync(l => l.LearnerID == learnerId && l.UserID == currentUserId);

                if (!ownsLearner)
                    return Forbid();
            }

            var result = await _statisticsService.GetIndividualLearnerStatisticsAsync(learnerId);

            if (result == null)
            {
                return NotFound($"Learner with ID {learnerId} was not found.");
            }

            return Ok(result);
        }
    }
}