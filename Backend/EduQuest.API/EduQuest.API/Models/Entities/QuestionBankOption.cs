namespace EduQuest.API.Models.Entities
{
    public class QuestionBankOption
    {
        public int QuestionBankOptionID { get; set; }
        public required string OptionText { get; set; }
        public bool IsCorrect { get; set; }

        public int QuestionBankItemID { get; set; }
        public QuestionBankItem? QuestionBankItem { get; set; }
    }
}