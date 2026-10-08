namespace EduQuest.API.DTOs
{
    public class QuizAttemptOptionDto
    {
        public int QuizOptionID { get; set; }
        public required string OptionText { get; set; }
    }
}