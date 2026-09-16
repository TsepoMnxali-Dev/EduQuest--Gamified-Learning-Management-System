namespace EduQuest.API.DTOs
{
    public class LeaderboardDto
    {
        public int LeaderboardID { get; set; }
        public int TotalPoints { get; set; }
        public required string Rank { get; set; }
        public required string LastUpdated { get; set; }
        public int LearnerID { get; set; }
    }
}