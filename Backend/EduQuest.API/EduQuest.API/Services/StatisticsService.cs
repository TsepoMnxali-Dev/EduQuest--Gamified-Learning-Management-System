using EduQuest.API.Data;
using EduQuest.API.DTOs.Statistics;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDBContext _context;

        public StatisticsService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<StatisticsOverviewDto> GetOverviewAsync()
        {
            var totalLearners = await _context.Learners.CountAsync();

            var activeLearners = await _context.Learners
                .CountAsync(l => l.User != null && l.User.IsActive);

            var inactiveLearners = await _context.Learners
                .CountAsync(l => l.User != null && !l.User.IsActive);

            var totalQuizAttempts = await _context.QuizAttempts.CountAsync();

            var totalCompetitionParticipants =
                await _context.CompetitionLearners.CountAsync();

            var totalAchievementsEarned =
                await _context.LearnerAchievements.CountAsync();

            return new StatisticsOverviewDto
            {
                TotalLearners = totalLearners,
                ActiveLearners = activeLearners,
                InactiveLearners = inactiveLearners,
                TotalQuizAttempts = totalQuizAttempts,
                TotalCompetitionParticipants = totalCompetitionParticipants,
                TotalAchievementsEarned = totalAchievementsEarned
            };
        }

        public async Task<List<LearnerCountByProvinceDto>> GetLearnersByProvinceAsync()
        {
            return await _context.Learners
                .AsNoTracking()
                .GroupBy(l => new
                {
                    l.School.ProvinceID,
                    l.School.Province.ProvinceName
                })
                .Select(g => new LearnerCountByProvinceDto
                {
                    ProvinceID = g.Key.ProvinceID,
                    ProvinceName = g.Key.ProvinceName,
                    LearnerCount = g.Count()
                })
                .OrderBy(x => x.ProvinceName)
                .ToListAsync();
        }

        public async Task<List<LearnerCountBySchoolDto>> GetLearnersBySchoolAsync()
        {
            return await _context.Learners
                .AsNoTracking()
                .GroupBy(l => new
                {
                    l.SchoolID,
                    l.School.SchoolName
                })
                .Select(g => new LearnerCountBySchoolDto
                {
                    SchoolID = g.Key.SchoolID,
                    SchoolName = g.Key.SchoolName,
                    LearnerCount = g.Count()
                })
                .OrderBy(x => x.SchoolName)
                .ToListAsync();
        }

        public async Task<List<LearnerCountByGradeDto>> GetLearnersByGradeAsync()
        {
            return await _context.Learners
                .AsNoTracking()
                .GroupBy(l => new
                {
                    l.GradeID,
                    l.Grade.GradeName
                })
                .Select(g => new LearnerCountByGradeDto
                {
                    GradeID = g.Key.GradeID,
                    GradeName = g.Key.GradeName,
                    LearnerCount = g.Count()
                })
                .OrderBy(x => x.GradeID)
                .ToListAsync();
        }

        public async Task<List<AverageScoreByProvinceDto>> GetAverageScoreByProvinceAsync()
        {
            return await _context.QuizAttempts
                .AsNoTracking()
                .GroupBy(a => new
                {
                    a.Learner.School.ProvinceID,
                    a.Learner.School.Province.ProvinceName
                })
                .Select(g => new AverageScoreByProvinceDto
                {
                    ProvinceID = g.Key.ProvinceID,
                    ProvinceName = g.Key.ProvinceName,
                    AverageScore = g.Average(a => a.Score)
                })
                .OrderBy(x => x.ProvinceName)
                .ToListAsync();
        }

        public async Task<List<AverageScoreBySchoolDto>> GetAverageScoreBySchoolAsync()
        {
            return await _context.QuizAttempts
                .AsNoTracking()
                .GroupBy(a => new
                {
                    a.Learner.SchoolID,
                    a.Learner.School.SchoolName
                })
                .Select(g => new AverageScoreBySchoolDto
                {
                    SchoolID = g.Key.SchoolID,
                    SchoolName = g.Key.SchoolName,
                    AverageScore = g.Average(a => a.Score)
                })
                .OrderBy(x => x.SchoolName)
                .ToListAsync();
        }

        public async Task<List<AverageScoreByGradeDto>> GetAverageScoreByGradeAsync()
        {
            return await _context.QuizAttempts
                .AsNoTracking()
                .GroupBy(a => new
                {
                    a.Learner.GradeID,
                    a.Learner.Grade.GradeName
                })
                .Select(g => new AverageScoreByGradeDto
                {
                    GradeID = g.Key.GradeID,
                    GradeName = g.Key.GradeName,
                    AverageScore = g.Average(a => a.Score)
                })
                .OrderBy(x => x.GradeID)
                .ToListAsync();
        }

        public async Task<List<AverageScoreBySubjectDto>> GetAverageScoreBySubjectAsync()
        {
            return await _context.QuizAttempts
                .AsNoTracking()
                .GroupBy(a => new
                {
                    a.Quiz.Topic.SubjectID,
                    a.Quiz.Topic.Subject.SubjectName
                })
                .Select(g => new AverageScoreBySubjectDto
                {
                    SubjectID = g.Key.SubjectID,
                    SubjectName = g.Key.SubjectName,
                    AverageScore = g.Average(a => a.Score)
                })
                .OrderBy(x => x.SubjectName)
                .ToListAsync();
        }

        public async Task<List<AverageScoreByTopicDto>> GetAverageScoreByTopicAsync()
        {
            return await _context.QuizAttempts
                .AsNoTracking()
                .GroupBy(a => new
                {
                    a.Quiz.TopicID,
                    a.Quiz.Topic.TopicName
                })
                .Select(g => new AverageScoreByTopicDto
                {
                    TopicID = g.Key.TopicID,
                    TopicName = g.Key.TopicName,
                    AverageScore = g.Average(a => a.Score)
                })
                .OrderBy(x => x.TopicName)
                .ToListAsync();
        }

        public async Task<PassFailStatisticsDto> GetPassFailStatisticsAsync()
        {
            var totalAttempts = await _context.QuizAttempts.CountAsync();

            var passedAttempts = await _context.QuizAttempts
                .CountAsync(a => a.Score >= 50);

            var failedAttempts = await _context.QuizAttempts
                .CountAsync(a => a.Score < 50);

            var passRate = totalAttempts == 0
                ? 0
                : (double)passedAttempts / totalAttempts * 100;

            var failRate = totalAttempts == 0
                ? 0
                : (double)failedAttempts / totalAttempts * 100;

            return new PassFailStatisticsDto
            {
                TotalAttempts = totalAttempts,
                PassedAttempts = passedAttempts,
                FailedAttempts = failedAttempts,
                PassRate = passRate,
                FailRate = failRate
            };
        }
        public async Task<QuizStatisticsDto> GetQuizAttemptsStatisticsAsync()
        {
            var totalAttempts = await _context.QuizAttempts
                .CountAsync();

            var uniqueLearners = await _context.QuizAttempts
                .Select(a => a.LearnerID)
                .Distinct()
                .CountAsync();

            return new QuizStatisticsDto
            {
                TotalAttempts = totalAttempts,
                UniqueLearners = uniqueLearners
            };
        }

        public async Task<QuizCompletionStatisticsDto> GetQuizCompletionStatisticsAsync()
        {
            var totalCompleted = await _context.QuizAttempts
                .CountAsync();

            var uniqueLearners = await _context.QuizAttempts
                .Select(a => a.LearnerID)
                .Distinct()
                .CountAsync();

            return new QuizCompletionStatisticsDto
            {
                TotalCompleted = totalCompleted,
                UniqueLearners = uniqueLearners
            };
        }
        public async Task<CompetitionStatisticsDto> GetCompetitionStatisticsAsync()
        {
            var totalParticipations = await _context.CompetitionLearners
                .CountAsync();

            var uniqueLearners = await _context.CompetitionLearners
                .Select(cl => cl.LearnerID)
                .Distinct()
                .CountAsync();

            return new CompetitionStatisticsDto
            {
                TotalParticipations = totalParticipations,
                UniqueLearners = uniqueLearners
            };
        }
        public async Task<AchievementStatisticsDto> GetAchievementStatisticsAsync()
        {
            var totalAchievementsEarned = await _context.LearnerAchievements
                .CountAsync();

            var uniqueLearnersWithAchievements = await _context.LearnerAchievements
                .Select(la => la.LearnerID)
                .Distinct()
                .CountAsync();

            return new AchievementStatisticsDto
            {
                TotalAchievementsEarned = totalAchievementsEarned,
                UniqueLearnersWithAchievements = uniqueLearnersWithAchievements
            };
        }
        public async Task<IndividualLearnerStatisticsDto?> GetIndividualLearnerStatisticsAsync(int learnerId)
        {
            var learner = await _context.Learners
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Grade)
                .Include(l => l.School)
                    .ThenInclude(s => s.Province)
                .FirstOrDefaultAsync(l => l.LearnerID == learnerId);

            if (learner == null)
            {
                return null;
            }

            var quizAttempts = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.LearnerID == learnerId)
                .ToListAsync();

            var totalQuizAttempts = quizAttempts.Count;

            var averageQuizScore = totalQuizAttempts == 0
                ? 0
                : quizAttempts.Average(a => a.Score);

            var passedQuizzes = quizAttempts
                .Count(a => a.Score >= 50);

            var failedQuizzes = quizAttempts
                .Count(a => a.Score < 50);

            var competitionsParticipated = await _context.CompetitionLearners
                .CountAsync(cl => cl.LearnerID == learnerId);

            var achievementsEarned = await _context.LearnerAchievements
                .CountAsync(la => la.LearnerID == learnerId);

            return new IndividualLearnerStatisticsDto
            {
                LearnerID = learner.LearnerID,

                LearnerName = learner.User == null
                    ? string.Empty
                    : $"{learner.User.FirstName} {learner.User.LastName}",

                GradeName = learner.Grade.GradeName,

                SchoolName = learner.School.SchoolName,

                ProvinceName = learner.School.Province.ProvinceName,

                TotalQuizAttempts = totalQuizAttempts,

                AverageQuizScore = averageQuizScore,

                PassedQuizzes = passedQuizzes,

                FailedQuizzes = failedQuizzes,

                CompetitionsParticipated = competitionsParticipated,

                AchievementsEarned = achievementsEarned
            };
        }
    }
}