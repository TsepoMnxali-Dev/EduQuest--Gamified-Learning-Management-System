namespace EduQuest.API.Models.Entities
{
    public class LeaderBoard
    {
        public int LeaderboardID { get; set; }
        public int TotalPoints { get; set; }
        public required string Rank  { get; set; }
        public required string LastUpdated { get; set; }

        public int LearnerID { get; set; }
        public Learner? Learner { get; set; }
    }
}
