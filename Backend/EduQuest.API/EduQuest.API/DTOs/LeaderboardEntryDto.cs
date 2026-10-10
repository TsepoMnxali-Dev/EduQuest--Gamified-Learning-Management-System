namespace EduQuest.API.DTOs
{
    // One row on the live leaderboard, calculated from quiz attempts.
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int LearnerID { get; set; }
        public required string DisplayName { get; set; }
        public required string SchoolName { get; set; }
        public int ProvinceID { get; set; }
        public required string ProvinceName { get; set; }
        public required string GradeName { get; set; }

        // Total correct answers across the quiz attempts in the chosen period/subject.
        public int Points { get; set; }
        public int QuizzesTaken { get; set; }
        // Average quiz percentage (0-100) across those attempts.
        public double AveragePercent { get; set; }

        // True for the row that belongs to the signed-in learner.
        public bool IsCurrentUser { get; set; }
    }
}