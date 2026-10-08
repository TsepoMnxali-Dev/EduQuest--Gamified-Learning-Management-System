namespace EduQuest.API.Models.Entities
{
    public class QuizAttempt
    {
        public int QuizAttemptID { get; set; }
        public int Score { get; set; }
        public required DateTime DateTaken { get; set; }
        public required TimeSpan TimeTaken { get; set; }


        public int QuizID { get; set; }
        public Quiz? Quiz { get; set; }

        public int LearnerID { get; set; }
        public Learner? Learner { get; set; }

        public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
        public ICollection<QuizAttemptQuestion> QuizAttemptQuestions
        { get; set; } = new List<QuizAttemptQuestion>();
    }
}
