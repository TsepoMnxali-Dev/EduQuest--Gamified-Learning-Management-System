namespace EduQuest.API.Models.Entities
{
    public class QuizAttempt
    {
        public int QuizAttemptID { get; set; }
        public int Score { get; set; }
        public required string DateTaken { get; set; }
        public required string TimeTaken { get; set; }

        public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();

        public int QuizID { get; set; }
        public Quiz? Quiz { get; set; }

        public int LearnerID { get; set; }
        public Learner? Learner { get; set; }
    }
}
