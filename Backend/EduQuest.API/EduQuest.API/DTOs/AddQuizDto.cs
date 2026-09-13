namespace EduQuest.API.DTOs
{
    public class AddQuizDto
    {
        public required string QuizTitle { get; set; }
        public required string Difficulty { get; set; }
        public required string TimeLimit { get; set; }
        public bool IsPublished { get; set; }
    }
}
