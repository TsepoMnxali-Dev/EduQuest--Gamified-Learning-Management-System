namespace EduQuest.API.DTOs
{
    public class QuestionBankItemDto
    {
        public int QuestionBankItemID { get; set; }
        public int TopicID { get; set; }
        public string? TopicName { get; set; }
        public required string QuestionText { get; set; }
        public string? SourceExtract { get; set; }
        public required string Explanation { get; set; }
        public required string Difficulty { get; set; }
        public required string Source { get; set; }
        public List<QuestionBankOptionDto> Options { get; set; } = new();
    }
}

