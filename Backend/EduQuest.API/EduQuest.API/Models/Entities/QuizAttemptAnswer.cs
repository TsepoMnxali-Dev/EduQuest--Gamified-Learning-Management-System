namespace EduQuest.API.Models.Entities
{
    public class QuizAttemptAnswer
    {
        public int AnswerID { get; set; }


        public int QuizOptionID { get; set; }
        public QuizOption? QuizOption { get; set; }


        public int QuestionID { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }


        public int AttemptID { get; set; }
        public QuizAttempt? QuizAttempt { get; set; }


    }
}
