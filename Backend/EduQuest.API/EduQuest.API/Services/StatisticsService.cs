
using EduQuest.API.Data;
using EduQuest.API.DTOs.Statistics;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Services
{
    public class StatisticsService : IStatisticsService
    {
        private const double PassThreshold = 50.0;

        private readonly ApplicationDBContext _context;

        public StatisticsService(ApplicationDBContext context)
        {
            _context = context;
        }

        // =====================================================
        // SHARED HELPER: COMPLETED QUIZ ATTEMPTS
        // =====================================================
        // Score is the number of correct answers, not a percentage.
        //
        // Only attempts with saved answers are treated as submitted.
        // QuizAttemptQuestions supplies the number of questions
        // assigned to the attempt.
        //
        // Attempts without a valid question count are excluded
        // from percentage-based reports to avoid false results.

        private sealed class CompletedAttempt
        {
            public int LearnerID { get; set; }

            public int ProvinceID { get; set; }
            public string ProvinceName { get; set; } = string.Empty;

            public int SchoolID { get; set; }
            public string SchoolName { get; set; } = string.Empty;

            public int GradeID { get; set; }
            public string GradeName { get; set; } = string.Empty;

            public int SubjectID { get; set; }
            public string SubjectName { get; set; } = string.Empty;

            public int TopicID { get; set; }
            public string TopicName { get; set; } = string.Empty;

            public double ScorePercentage { get; set; }

            public bool Passed => ScorePercentage >= PassThreshold;
        }

        private async Task<List<CompletedAttempt>>
            GetCompletedAttemptsAsync(int? learnerId = null)
        {
            var query = _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.QuizAttemptAnswers.Any())
                .Where(a => a.QuizAttemptQuestions.Any());

            if (learnerId.HasValue)
            {
                query = query.Where(
                    a => a.LearnerID == learnerId.Value
                );
            }

            var attempts = await query
                .Select(a => new
                {
                    a.LearnerID,

                    ProvinceID = a.Learner.School.ProvinceID,
                    ProvinceName =
                        a.Learner.School.Province.ProvinceName,

                    a.Learner.SchoolID,
                    SchoolName = a.Learner.School.SchoolName,

                    a.Learner.GradeID,
                    GradeName = a.Learner.Grade.GradeName,

                    SubjectID = a.Quiz.Topic.SubjectID,
                    SubjectName =
                        a.Quiz.Topic.Subject.SubjectName,

                    a.Quiz.TopicID,
                    TopicName = a.Quiz.Topic.TopicName,

                    a.Score,

                    TotalQuestions =
                        a.QuizAttemptQuestions.Count()
                })
                .ToListAsync();

            return attempts
                .Where(a => a.TotalQuestions > 0)
                .Select(a => new CompletedAttempt
                {
                    LearnerID = a.LearnerID,

                    ProvinceID = a.ProvinceID,
                    ProvinceName = a.ProvinceName,

                    SchoolID = a.SchoolID,
                    SchoolName = a.SchoolName,

                    GradeID = a.GradeID,
                    GradeName = a.GradeName,

                    SubjectID = a.SubjectID,
                    SubjectName = a.SubjectName,

                    TopicID = a.TopicID,
                    TopicName = a.TopicName,

                    ScorePercentage =
                        Math.Clamp(
                            (double)a.Score / a.TotalQuestions * 100.0,
                            0.0,
                            100.0
                        )
                })
                .ToList();
        }

        // =====================================================
        // PLATFORM OVERVIEW
        // =====================================================

        public async Task<StatisticsOverviewDto> GetOverviewAsync()
        {
            var totalLearners = await _context.Learners
                .CountAsync();

            var activeLearners = await _context.Learners
                .CountAsync(l =>
                    l.User != null && l.User.IsActive);

            var inactiveLearners = await _context.Learners
                .CountAsync(l =>
                    l.User != null && !l.User.IsActive);

            var totalQuizAttempts = await _context.QuizAttempts
                .CountAsync();

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
                TotalCompetitionParticipants =
                    totalCompetitionParticipants,
                TotalAchievementsEarned = totalAchievementsEarned
            };
        }

        // =====================================================
        // LEARNER DISTRIBUTION BY PROVINCE
        // =====================================================

        public async Task<List<LearnerCountByProvinceDto>>
            GetLearnersByProvinceAsync()
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

        // =====================================================
        // LEARNER DISTRIBUTION BY SCHOOL
        // =====================================================

        public async Task<List<LearnerCountBySchoolDto>>
            GetLearnersBySchoolAsync()
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

        // =====================================================
        // LEARNER DISTRIBUTION BY GRADE
        // =====================================================

        public async Task<List<LearnerCountByGradeDto>>
            GetLearnersByGradeAsync()
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

        // =====================================================
        // AVERAGE PERFORMANCE BY PROVINCE
        // =====================================================

        public async Task<List<AverageScoreByProvinceDto>>
            GetAverageScoreByProvinceAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            return attempts
                .GroupBy(a => new
                {
                    a.ProvinceID,
                    a.ProvinceName
                })
                .Select(g => new AverageScoreByProvinceDto
                {
                    ProvinceID = g.Key.ProvinceID,
                    ProvinceName = g.Key.ProvinceName,
                    AverageScore =
                        g.Average(a => a.ScorePercentage)
                })
                .OrderBy(x => x.ProvinceName)
                .ToList();
        }

        // =====================================================
        // AVERAGE PERFORMANCE BY SCHOOL
        // =====================================================

        public async Task<List<AverageScoreBySchoolDto>>
            GetAverageScoreBySchoolAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            return attempts
                .GroupBy(a => new
                {
                    a.SchoolID,
                    a.SchoolName
                })
                .Select(g => new AverageScoreBySchoolDto
                {
                    SchoolID = g.Key.SchoolID,
                    SchoolName = g.Key.SchoolName,
                    AverageScore =
                        g.Average(a => a.ScorePercentage)
                })
                .OrderBy(x => x.SchoolName)
                .ToList();
        }

        // =====================================================
        // AVERAGE PERFORMANCE BY GRADE
        // =====================================================

        public async Task<List<AverageScoreByGradeDto>>
            GetAverageScoreByGradeAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            return attempts
                .GroupBy(a => new
                {
                    a.GradeID,
                    a.GradeName
                })
                .Select(g => new AverageScoreByGradeDto
                {
                    GradeID = g.Key.GradeID,
                    GradeName = g.Key.GradeName,
                    AverageScore =
                        g.Average(a => a.ScorePercentage)
                })
                .OrderBy(x => x.GradeID)
                .ToList();
        }

        // =====================================================
        // AVERAGE PERFORMANCE BY SUBJECT
        // =====================================================

        public async Task<List<AverageScoreBySubjectDto>>
            GetAverageScoreBySubjectAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            return attempts
                .GroupBy(a => new
                {
                    a.SubjectID,
                    a.SubjectName
                })
                .Select(g => new AverageScoreBySubjectDto
                {
                    SubjectID = g.Key.SubjectID,
                    SubjectName = g.Key.SubjectName,
                    AverageScore =
                        g.Average(a => a.ScorePercentage)
                })
                .OrderBy(x => x.SubjectName)
                .ToList();
        }

        // =====================================================
        // AVERAGE PERFORMANCE BY TOPIC
        // =====================================================

        public async Task<List<AverageScoreByTopicDto>>
            GetAverageScoreByTopicAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            return attempts
                .GroupBy(a => new
                {
                    a.TopicID,
                    a.TopicName
                })
                .Select(g => new AverageScoreByTopicDto
                {
                    TopicID = g.Key.TopicID,
                    TopicName = g.Key.TopicName,
                    AverageScore =
                        g.Average(a => a.ScorePercentage)
                })
                .OrderBy(x => x.TopicName)
                .ToList();
        }

        // =====================================================
        // QUIZ PASS / FAIL STATISTICS
        // =====================================================
        // Passing percentage: 50% or higher.

        public async Task<PassFailStatisticsDto>
            GetPassFailStatisticsAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            var totalAttempts = attempts.Count;

            var passedAttempts = attempts.Count(a => a.Passed);

            var failedAttempts = attempts.Count(a => !a.Passed);

            var passRate = totalAttempts == 0
                ? 0.0
                : (double)passedAttempts / totalAttempts * 100.0;

            var failRate = totalAttempts == 0
                ? 0.0
                : (double)failedAttempts / totalAttempts * 100.0;

            return new PassFailStatisticsDto
            {
                TotalAttempts = totalAttempts,
                PassedAttempts = passedAttempts,
                FailedAttempts = failedAttempts,
                PassRate = passRate,
                FailRate = failRate
            };
        }

        // =====================================================
        // QUIZ ATTEMPT ENGAGEMENT
        // =====================================================
        // Counts all started attempts, including unfinished ones.

        public async Task<QuizStatisticsDto>
            GetQuizAttemptsStatisticsAsync()
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

        // =====================================================
        // QUIZ COMPLETION ENGAGEMENT
        // =====================================================
        // Only submitted attempts are counted as completed.

        public async Task<QuizCompletionStatisticsDto>
            GetQuizCompletionStatisticsAsync()
        {
            var attempts = await GetCompletedAttemptsAsync();

            var totalCompleted = attempts.Count;

            var uniqueLearners = attempts
                .Select(a => a.LearnerID)
                .Distinct()
                .Count();

            return new QuizCompletionStatisticsDto
            {
                TotalCompleted = totalCompleted,
                UniqueLearners = uniqueLearners
            };
        }

        // =====================================================
        // COMPETITION ENGAGEMENT
        // =====================================================

        public async Task<CompetitionStatisticsDto>
            GetCompetitionStatisticsAsync()
        {
            var totalParticipations =
                await _context.CompetitionLearners.CountAsync();

            var uniqueLearners =
                await _context.CompetitionLearners
                    .Select(cl => cl.LearnerID)
                    .Distinct()
                    .CountAsync();

            return new CompetitionStatisticsDto
            {
                TotalParticipations = totalParticipations,
                UniqueLearners = uniqueLearners
            };
        }

        // =====================================================
        // ACHIEVEMENT ENGAGEMENT
        // =====================================================

        public async Task<AchievementStatisticsDto>
            GetAchievementStatisticsAsync()
        {
            var totalAchievementsEarned =
                await _context.LearnerAchievements.CountAsync();

            var uniqueLearnersWithAchievements =
                await _context.LearnerAchievements
                    .Select(la => la.LearnerID)
                    .Distinct()
                    .CountAsync();

            return new AchievementStatisticsDto
            {
                TotalAchievementsEarned = totalAchievementsEarned,
                UniqueLearnersWithAchievements =
                    uniqueLearnersWithAchievements
            };
        }

        // =====================================================
        // INDIVIDUAL LEARNER STATISTICS
        // =====================================================

        public async Task<IndividualLearnerStatisticsDto?>
            GetIndividualLearnerStatisticsAsync(int learnerId)
        {
            var learner = await _context.Learners
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Grade)
                .Include(l => l.School)
                    .ThenInclude(s => s.Province)
                .FirstOrDefaultAsync(
                    l => l.LearnerID == learnerId
                );

            if (learner == null)
            {
                return null;
            }

            var quizAttempts =
                await GetCompletedAttemptsAsync(learnerId);

            var totalQuizAttempts = quizAttempts.Count;

            var averageQuizScore = totalQuizAttempts == 0
                ? 0.0
                : quizAttempts.Average(a => a.ScorePercentage);

            var passedQuizzes = quizAttempts
                .Count(a => a.Passed);

            var failedQuizzes = quizAttempts
                .Count(a => !a.Passed);

            var competitionsParticipated =
                await _context.CompetitionLearners
                    .CountAsync(
                        cl => cl.LearnerID == learnerId
                    );

            var achievementsEarned =
                await _context.LearnerAchievements
                    .CountAsync(
                        la => la.LearnerID == learnerId
                    );

            return new IndividualLearnerStatisticsDto
            {
                LearnerID = learner.LearnerID,

                LearnerName = learner.User == null
                    ? string.Empty
                    : $"{learner.User.FirstName} {learner.User.LastName}",

                GradeName = learner.Grade.GradeName,

                SchoolName = learner.School.SchoolName,

                ProvinceName =
                    learner.School.Province.ProvinceName,

                TotalQuizAttempts = totalQuizAttempts,

                AverageQuizScore = averageQuizScore,

                PassedQuizzes = passedQuizzes,

                FailedQuizzes = failedQuizzes,

                CompetitionsParticipated =
                    competitionsParticipated,

                AchievementsEarned = achievementsEarned
            };
        }
    }
}
