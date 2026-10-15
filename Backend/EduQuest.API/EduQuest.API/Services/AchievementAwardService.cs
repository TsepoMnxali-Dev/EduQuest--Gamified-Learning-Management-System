using EduQuest.API.Data;
using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Services
{
    public sealed record AchievementSyncSummary(int SubmittedAttempts, int NewAwards);

    public static class AchievementAwardService
    {
        private sealed record BadgeDefinition(
            string Name, string Description, int PointsRequired, string Icon);

        private static readonly BadgeDefinition[] Defaults =
        {
            new("First Quiz", "Submitted your first quiz.", 0, "🎯"),
            new("Quiz Master", "Completed five different quizzes.", 0, "📚"),
            new("Perfect Score", "Earned 100% on a submitted quiz.", 0, "💯"),
            new("Knowledge Champion", "Earned 100 quiz points.", 100, "🏆")
        };

        public static async Task<AchievementSyncSummary> SyncForLearnerAsync(
            ApplicationDBContext context, int learnerId)
        {
            // Use the same explicit saved-answer test as the working
            // learner quiz progress and quiz submission endpoints.
            // Starting a quiz alone does NOT qualify for an achievement.
            var completed = await context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.LearnerID == learnerId &&
                    context.QuizAttemptAnswers.Any(answer =>
                        answer.AttemptID == a.QuizAttemptID))
                .Select(a => new
                {
                    a.QuizID,
                    a.Score,
                    QuestionCount = context.QuizAttemptQuestions.Count(question =>
                        question.QuizAttemptID == a.QuizAttemptID)
                })
                .ToListAsync();

            if (completed.Count == 0)
                return new AchievementSyncSummary(0, 0);

            var distinctQuizzes = completed.Select(a => a.QuizID).Distinct().Count();
            var totalPoints = completed.Sum(a => a.Score);
            var perfect = completed.Any(a =>
                a.QuestionCount > 0 && a.Score == a.QuestionCount);

            var achievements = await context.Achievements.ToListAsync();
            var addedDefinitions = false;

            // Treat previously created badge names case-insensitively.
            // Do not overwrite existing custom descriptions or icons.
            foreach (var badge in Defaults)
            {
                var exists = achievements.Any(a =>
                    string.Equals(a.Name?.Trim(), badge.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (exists) continue;

                var achievement = new Achievement
                {
                    Name = badge.Name,
                    Description = badge.Description,
                    PointsRequired = badge.PointsRequired,
                    BadgeImage = badge.Icon
                };
                context.Achievements.Add(achievement);
                achievements.Add(achievement);
                addedDefinitions = true;
            }

            if (addedDefinitions)
                await context.SaveChangesAsync();

            var ownedIds = (await context.LearnerAchievements
                .AsNoTracking()
                .Where(la => la.LearnerID == learnerId)
                .Select(la => la.AchievementID)
                .ToListAsync()).ToHashSet();

            var newAwards = 0;

            foreach (var achievement in achievements)
            {
                // Normalize names before comparing: existing database rows
                // may have different capitalization or surrounding spaces.
                var name = (achievement.Name ?? string.Empty).Trim();

                var eligible = name.ToUpperInvariant() switch
                {
                    "FIRST QUIZ" => true,
                    "QUIZ MASTER" => distinctQuizzes >= 5,
                    "PERFECT SCORE" => perfect,
                    "KNOWLEDGE CHAMPION" => totalPoints >= 100,
                    // Administrator-created 0-point badges are manual only.
                    _ => achievement.PointsRequired > 0 &&
                         totalPoints >= achievement.PointsRequired
                };

                if (!eligible || !ownedIds.Add(achievement.AchievementID))
                    continue;

                context.LearnerAchievements.Add(new LearnerAchievement
                {
                    LearnerID = learnerId,
                    AchievementID = achievement.AchievementID
                });
                newAwards++;
            }

            if (newAwards > 0)
                await context.SaveChangesAsync();

            return new AchievementSyncSummary(completed.Count, newAwards);
        }
    }
}
