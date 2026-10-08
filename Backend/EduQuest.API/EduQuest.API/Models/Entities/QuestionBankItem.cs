namespace EduQuest.API.Models.Entities
{
    public class QuestionBankItem
    {
        public int QuestionBankItemID { get; set; }

        public int TopicID { get; set; }
        public Topic? Topic { get; set; }

        public required string QuestionText { get; set; }
        public string? SourceExtract { get; set; }
        public required string Explanation { get; set; }
        public required string Difficulty { get; set; }
        public required string Source { get; set; }  // "ChatGPT", "Gemini", "Manual"

        public ICollection<QuestionBankOption> QuestionBankOptions { get; set; }
            = new List<QuestionBankOption>();
    }
}