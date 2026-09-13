namespace EduQuest.API.Models.Entities
{
    public class QuizOption
    {
        public int QuizOptionID { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }



        public int QuestionID { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }


        public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
    }
}
