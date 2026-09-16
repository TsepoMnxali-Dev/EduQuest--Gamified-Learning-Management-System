namespace EduQuest.API.DTOs
{
    public class QuizOptionDto
    {
        public int QuizOptionID { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionID { get; set; }
    }
}