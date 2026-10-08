namespace EduQuest.API.DTOs
{
    public class QuizAttemptResultQuestionDto
    {
        public int QuestionOrder { get; set; }

        public required string QuestionText { get; set; }

        public required string SelectedAnswer { get; set; }

        public required string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public required string Explanation { get; set; }
    }
}