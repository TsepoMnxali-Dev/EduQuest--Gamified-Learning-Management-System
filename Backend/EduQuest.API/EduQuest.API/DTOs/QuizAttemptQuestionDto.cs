namespace EduQuest.API.DTOs
{
    public class QuizAttemptQuestionDto
    {
        public int QuizAttemptQuestionID { get; set; }
        public int QuizQuestionID { get; set; }
        public int QuestionOrder { get; set; }

        public required string QuestionText { get; set; }

        public string? SourceExtract { get; set; }
        public List<QuizAttemptOptionDto> Options { get; set; } = new();
    }
}