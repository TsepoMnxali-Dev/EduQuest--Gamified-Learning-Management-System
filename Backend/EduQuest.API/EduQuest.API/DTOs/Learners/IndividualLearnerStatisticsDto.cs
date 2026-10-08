namespace EduQuest.API.DTOs.Statistics
{
    public class IndividualLearnerStatisticsDto
    {
        public int LearnerID { get; set; }

        public string LearnerName { get; set; } = string.Empty;

        public string GradeName { get; set; } = string.Empty;

        public string SchoolName { get; set; } = string.Empty;

        public string ProvinceName { get; set; } = string.Empty;

        public int TotalQuizAttempts { get; set; }

        public double AverageQuizScore { get; set; }

        public int PassedQuizzes { get; set; }

        public int FailedQuizzes { get; set; }

        public int CompetitionsParticipated { get; set; }

        public int AchievementsEarned { get; set; }
    }
}