namespace EduQuest.API.Models.Entities
{
    public class QuizQuestion
    {
        public int QuizQuestionID { get; set; }

        public required string QuestionText { get; set; }

        public string? SourceExtract { get; set; }

        public required string Explanation { get; set; }

        public bool GeneratedByAI { get; set; }
        public bool ApprovedByAdmin { get; set; }

        public int QuizID { get; set; }
        public Quiz? Quiz { get; set; }

        public ICollection<QuizOption> QuizOptions { get; set; }
    = new List<QuizOption>();

        public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
            = new HashSet<QuizAttemptAnswer>();

        public ICollection<QuizAttemptQuestion> QuizAttemptQuestions { get; set; }
            = new List<QuizAttemptQuestion>();
    }
}