using EduQuest.API.DTOs.Statistics;

namespace EduQuest.API.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsOverviewDto> GetOverviewAsync();

        Task<List<LearnerCountByProvinceDto>> GetLearnersByProvinceAsync();

        Task<List<LearnerCountBySchoolDto>> GetLearnersBySchoolAsync();

        Task<List<LearnerCountByGradeDto>> GetLearnersByGradeAsync();

        Task<List<AverageScoreByProvinceDto>> GetAverageScoreByProvinceAsync();

        Task<List<AverageScoreBySchoolDto>> GetAverageScoreBySchoolAsync();

        Task<List<AverageScoreByGradeDto>> GetAverageScoreByGradeAsync();

        Task<List<AverageScoreBySubjectDto>> GetAverageScoreBySubjectAsync();

        Task<List<AverageScoreByTopicDto>> GetAverageScoreByTopicAsync();

        Task<PassFailStatisticsDto> GetPassFailStatisticsAsync();
        Task<QuizStatisticsDto> GetQuizAttemptsStatisticsAsync();

        Task<QuizCompletionStatisticsDto> GetQuizCompletionStatisticsAsync();
        Task<CompetitionStatisticsDto> GetCompetitionStatisticsAsync();
        Task<AchievementStatisticsDto> GetAchievementStatisticsAsync();


        Task<IndividualLearnerStatisticsDto?> GetIndividualLearnerStatisticsAsync(int learnerId);
    }
}