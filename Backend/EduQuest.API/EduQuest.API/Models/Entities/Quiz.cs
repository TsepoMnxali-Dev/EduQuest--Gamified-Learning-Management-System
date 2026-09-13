namespace EduQuest.API.Models.Entities
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public required string QuizTitle { get; set; }
        public required string Difficulty {  get; set; }
        public required string TimeLimit {  get; set; }
        public bool IsPublished { get; set; }

        // Foreign Key
        public int TopicID { get; set; }

        // Navigation Property
        public Topic? Topic { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}
