namespace EduQuest.API.DTOs
{
    public class QuizAttemptDto
    {
        public int QuizAttemptID { get; set; }
        public int Score { get; set; }
        public required string DateTaken { get; set; }
        public required string TimeTaken { get; set; }
        public int QuizID { get; set; }
        public int LearnerID { get; set; }
    }
}