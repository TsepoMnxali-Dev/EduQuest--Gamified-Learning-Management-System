namespace EduQuest.API.DTOs
{
    public class QuizAttemptResultDto
    {
        public int QuizAttemptID { get; set; }
        public int QuizID { get; set; }
        public required string QuizTitle { get; set; }
        public int LearnerID { get; set; }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }

        public required DateTime DateTaken { get; set; }
        public required TimeSpan TimeTaken { get; set; }

        public List<QuizAttemptResultQuestionDto> Questions { get; set; } = new();
    }
}