namespace EduQuest.API.DTOs
{
    public class CompetitionLearnerDto
    {
        public int CompetitionLearnerID { get; set; }
        public int CompetitionID { get; set; }
        public int LearnerID { get; set; }
        public int Score { get; set; }
        public int Position { get; set; }
    }
}