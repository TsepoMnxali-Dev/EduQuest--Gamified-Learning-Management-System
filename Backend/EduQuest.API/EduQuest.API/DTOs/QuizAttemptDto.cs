namespace EduQuest.API.DTOs
{
    public class QuizAttemptDto
    {
        public int QuizAttemptID { get; set; }
        public int Score { get; set; }
        public required DateTime DateTaken { get; set; }
        public required TimeSpan TimeTaken { get; set; }
        public int QuizID { get; set; }
        public int LearnerID { get; set; }
    }
}