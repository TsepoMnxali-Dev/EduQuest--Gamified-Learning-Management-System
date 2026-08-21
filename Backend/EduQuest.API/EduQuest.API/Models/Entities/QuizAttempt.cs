namespace EduQuest.API.Models.Entities
{
    public class QuizAttempt
    {
        public int AttemptID { get; set; }
        public int Score { get; set; }
        public required string DateTaken { get; set; }
        public required string TimeTaken { get; set; }

        public ICollection<QuizAttemptAnswer>? QuizAttemptAnswers { get; set; }

        public int QuizID { get; set; }
        public Quiz? Quiz { get; set; }
    }
}
