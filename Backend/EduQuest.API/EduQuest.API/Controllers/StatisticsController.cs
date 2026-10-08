using EduQuest.API.DTOs.Statistics;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<StatisticsOverviewDto>> GetOverview()
        {
            var result = await _statisticsService.GetOverviewAsync();

            return Ok(result);
        }

        [HttpGet("learners/by-province")]
        public async Task<ActionResult<List<LearnerCountByProvinceDto>>> GetLearnersByProvince()
        {
            var result = await _statisticsService.GetLearnersByProvinceAsync();

            return Ok(result);
        }

        [HttpGet("learners/by-school")]
        public async Task<ActionResult<List<LearnerCountBySchoolDto>>> GetLearnersBySchool()
        {
            var result = await _statisticsService.GetLearnersBySchoolAsync();

            return Ok(result);
        }

        [HttpGet("learners/by-grade")]
        public async Task<ActionResult<List<LearnerCountByGradeDto>>> GetLearnersByGrade()
        {
            var result = await _statisticsService.GetLearnersByGradeAsync();

            return Ok(result);
        }
        [HttpGet("performance/by-province")]
        public async Task<ActionResult<List<AverageScoreByProvinceDto>>> GetAverageScoreByProvince()
        {
            var result = await _statisticsService.GetAverageScoreByProvinceAsync();

            return Ok(result);
        }

        [HttpGet("performance/by-school")]
        public async Task<ActionResult<List<AverageScoreBySchoolDto>>> GetAverageScoreBySchool()
        {
            var result = await _statisticsService.GetAverageScoreBySchoolAsync();

            return Ok(result);
        }

        [HttpGet("performance/by-grade")]
        public async Task<ActionResult<List<AverageScoreByGradeDto>>> GetAverageScoreByGrade()
        {
            var result = await _statisticsService.GetAverageScoreByGradeAsync();

            return Ok(result);
        }

        [HttpGet("performance/by-subject")]
        public async Task<ActionResult<List<AverageScoreBySubjectDto>>> GetAverageScoreBySubject()
        {
            var result = await _statisticsService.GetAverageScoreBySubjectAsync();

            return Ok(result);
        }

        [HttpGet("performance/by-topic")]
        public async Task<ActionResult<List<AverageScoreByTopicDto>>> GetAverageScoreByTopic()
        {
            var result = await _statisticsService.GetAverageScoreByTopicAsync();

            return Ok(result);
        }

        [HttpGet("performance/pass-fail")]
        public async Task<ActionResult<PassFailStatisticsDto>> GetPassFailStatistics()
        {
            var result = await _statisticsService.GetPassFailStatisticsAsync();

            return Ok(result);
        }
        [HttpGet("engagement/quiz-attempts")]
        public async Task<ActionResult<QuizStatisticsDto>> GetQuizAttemptsStatistics()
        {
            var result = await _statisticsService.GetQuizAttemptsStatisticsAsync();

            return Ok(result);
        }

        [HttpGet("engagement/quiz-completion")]
        public async Task<ActionResult<QuizCompletionStatisticsDto>> GetQuizCompletionStatistics()
        {
            var result = await _statisticsService.GetQuizCompletionStatisticsAsync();

            return Ok(result);
        }
        [HttpGet("engagement/competitions")]
        public async Task<ActionResult<CompetitionStatisticsDto>> GetCompetitionStatistics()
        {
            var result = await _statisticsService.GetCompetitionStatisticsAsync();

            return Ok(result);
        }
        [HttpGet("engagement/achievements")]
        public async Task<ActionResult<AchievementStatisticsDto>> GetAchievementStatistics()
        {
            var result = await _statisticsService.GetAchievementStatisticsAsync();

            return Ok(result);
        }
        [HttpGet("learners/{learnerId}")]
        public async Task<ActionResult<IndividualLearnerStatisticsDto>> GetIndividualLearnerStatistics(int learnerId)
        {
            var result = await _statisticsService.GetIndividualLearnerStatisticsAsync(learnerId);

            if (result == null)
            {
                return NotFound($"Learner with ID {learnerId} was not found.");
            }

            return Ok(result);
        }
    }
}