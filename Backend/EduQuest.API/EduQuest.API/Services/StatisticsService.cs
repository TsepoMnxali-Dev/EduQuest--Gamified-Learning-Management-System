using EduQuest.API.Data;
using EduQuest.API.DTOs.Statistics;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Services
{
    public class StatisticsService : IStatisticsService
    {
        /* A quiz attempt passes at 50% or more. */
        private const double PassMarkPercentage = 50;

        private readonly ApplicationDBContext _context;

        /* One finished quiz attempt, with its score as a percentage. */
        private sealed record AttemptScore(
            int LearnerID,
            int SchoolID, string SchoolName,
            int ProvinceID, string ProvinceName,
            int GradeID, string GradeName,
            int SubjectID, string SubjectName,
            int TopicID, string TopicName,
            double Percentage);

        public StatisticsService(ApplicationDBContext context)
        {
            _context = context;
        }

        /* QuizAttempt.Score is the NUMBER OF CORRECT ANSWERS (see QuizAttemptsController),
           not a percentage, so it is converted using the attempt's question count.
           Attempts are created with Score = 0 when a learner starts a quiz, so only
           attempts that have submitted answers count as completed. The grouping is done
           in memory because SQL can't aggregate over a per-row sub-count. */
        private async Task<List<AttemptScore>> GetCompletedAttemptScoresAsync(int? learnerId = null)
        {
            var rows = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.QuizAttemptAnswers.Any())
                .Where(a => !learnerId.HasValue || a.LearnerID == learnerId.Value)
                .Select(a => new
                {
                    a.LearnerID,
                    a.Learner.SchoolID,
                    a.Learner.School.SchoolName,
                    a.Learner.School.ProvinceID,
                    a.Learner.School.Province.ProvinceName,
                    a.Learner.GradeID,
                    a.Learner.Grade.GradeName,
                    a.Quiz.Topic.SubjectID,
                    a.Quiz.Topic.Subject.SubjectName,
                    a.Quiz.TopicID,
                    a.Quiz.Topic.TopicName,
                    a.Score,
                    TotalQuestions = a.QuizAttemptQuestions.Count
                })
                .ToListAsync();

            return rows
                .Select(r => new AttemptScore(
                    r.LearnerID,
                    r.SchoolID, r.SchoolName,
                    r.ProvinceID, r.ProvinceName,
                    r.GradeID, r.GradeName,
                    r.SubjectID, r.SubjectName,
                    r.TopicID, r.TopicName,
                    r.TotalQuestions > 0 ? r.Score * 100.0 / r.TotalQuestions : 0))
                .ToList();
        }

        public async Task<StatisticsOverviewDto> GetOverviewAsync()
        {
            var totalLearners = await _context.Learners.CountAsync();

            var activeLearners = await _context.Learners
                .CountAsync(l => l.User != null && l.User.IsActive);

            var inactiveLearners = await _context.Learners
                .CountAsync(l => l.User != null && !l.User.IsActive);

            var totalQuizAttempts = await _context.QuizAttempts
                .CountAsync(a => a.QuizAttemptAnswers.Any());

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
            var attempts = await GetCompletedAttemptScoresAsync();

            return attempts
                .GroupBy(a => new { a.ProvinceID, a.ProvinceName })
                .Select(g => new AverageScoreByProvinceDto
                {
                    ProvinceID = g.Key.ProvinceID,
                    ProvinceName = g.Key.ProvinceName,
                    AverageScore = g.Average(a => a.Percentage)
                })
                .OrderBy(x => x.ProvinceName)
                .ToList();
        }

        public async Task<List<AverageScoreBySchoolDto>> GetAverageScoreBySchoolAsync()
        {
            var attempts = await GetCompletedAttemptScoresAsync();

            return attempts
                .GroupBy(a => new { a.SchoolID, a.SchoolName })
                .Select(g => new AverageScoreBySchoolDto
                {
                    SchoolID = g.Key.SchoolID,
                    SchoolName = g.Key.SchoolName,
                    AverageScore = g.Average(a => a.Percentage)
                })
                .OrderBy(x => x.SchoolName)
                .ToList();
        }

        public async Task<List<AverageScoreByGradeDto>> GetAverageScoreByGradeAsync()
        {
            var attempts = await GetCompletedAttemptScoresAsync();

            return attempts
                .GroupBy(a => new { a.GradeID, a.GradeName })
                .Select(g => new AverageScoreByGradeDto
                {
                    GradeID = g.Key.GradeID,
                    GradeName = g.Key.GradeName,
                    AverageScore = g.Average(a => a.Percentage)
                })
                .OrderBy(x => x.GradeID)
                .ToList();
        }

        public async Task<List<AverageScoreBySubjectDto>> GetAverageScoreBySubjectAsync()
        {
            var attempts = await GetCompletedAttemptScoresAsync();

            return attempts
                .GroupBy(a => new { a.SubjectID, a.SubjectName })
                .Select(g => new AverageScoreBySubjectDto
                {
                    SubjectID = g.Key.SubjectID,
                    SubjectName = g.Key.SubjectName,
                    AverageScore = g.Average(a => a.Percentage)
                })
                .OrderBy(x => x.SubjectName)
                .ToList();
        }

        public async Task<List<AverageScoreByTopicDto>> GetAverageScoreByTopicAsync()
        {
            var attempts = await GetCompletedAttemptScoresAsync();

            return attempts
                .GroupBy(a => new { a.TopicID, a.TopicName })
                .Select(g => new AverageScoreByTopicDto
                {
                    TopicID = g.Key.TopicID,
                    TopicName = g.Key.TopicName,
                    AverageScore = g.Average(a => a.Percentage)
                })
                .OrderBy(x => x.TopicName)
                .ToList();
        }

        public async Task<PassFailStatisticsDto> GetPassFailStatisticsAsync()
        {
            var attempts = await GetCompletedAttemptScoresAsync();

            var totalAttempts = attempts.Count;
            var passedAttempts = attempts.Count(a => a.Percentage >= PassMarkPercentage);
            var failedAttempts = totalAttempts - passedAttempts;

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

        /* Unlike GetQuizAttemptsStatisticsAsync (every attempt that was started),
           this counts only attempts the learner actually submitted. */
        public async Task<QuizCompletionStatisticsDto> GetQuizCompletionStatisticsAsync()
        {
            var completed = await _context.QuizAttempts
                .Where(a => a.QuizAttemptAnswers.Any())
                .Select(a => a.LearnerID)
                .ToListAsync();

            return new QuizCompletionStatisticsDto
            {
                TotalCompleted = completed.Count,
                UniqueLearners = completed.Distinct().Count()
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

            /* Completed attempts only, with scores as percentages. */
            var quizAttempts = await GetCompletedAttemptScoresAsync(learnerId);

            var totalQuizAttempts = quizAttempts.Count;

            var averageQuizScore = totalQuizAttempts == 0
                ? 0
                : quizAttempts.Average(a => a.Percentage);

            var passedQuizzes = quizAttempts
                .Count(a => a.Percentage >= PassMarkPercentage);

            var failedQuizzes = totalQuizAttempts - passedQuizzes;

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