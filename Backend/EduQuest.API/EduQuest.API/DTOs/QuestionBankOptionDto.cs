namespace EduQuest.API.DTOs
{
    public class QuestionBankOptionDto
    {
        public int QuestionBankOptionID { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
