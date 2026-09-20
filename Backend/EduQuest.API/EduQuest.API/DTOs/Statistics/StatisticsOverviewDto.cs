namespace EduQuest.API.DTOs.Statistics
{
    public class StatisticsOverviewDto
    {
        public int TotalLearners { get; set; }
        public int ActiveLearners { get; set; }
        public int InactiveLearners { get; set; }
        public int TotalQuizAttempts { get; set; }
        public int TotalCompetitionParticipants { get; set; }
        public int TotalAchievementsEarned { get; set; }
    }
}