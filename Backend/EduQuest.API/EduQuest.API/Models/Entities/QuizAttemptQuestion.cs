namespace EduQuest.API.Models.Entities
{
    public class QuizAttemptQuestion
    {
        public int QuizAttemptQuestionID { get; set; }

        public int QuizAttemptID { get; set; }
        public QuizAttempt? QuizAttempt { get; set; }

        public int QuizQuestionID { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }

        public int QuestionOrder { get; set; }
    }
}