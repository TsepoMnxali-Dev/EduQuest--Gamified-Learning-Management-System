namespace EduQuest.API.DTOs
{
    public class QuizDto
    {
        public int QuizID { get; set; }
        public required string QuizTitle { get; set; }
        public required string Difficulty { get; set; }
        public required string TimeLimit { get; set; }
        public bool IsPublished { get; set; }
        public int TopicID { get; set; }
    }
}