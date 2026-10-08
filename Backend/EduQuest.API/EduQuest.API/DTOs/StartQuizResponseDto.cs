namespace EduQuest.API.DTOs
{
    public class StartQuizResponseDto
    {
        public int QuizAttemptID { get; set; }
        public int QuizID { get; set; }
        public int LearnerID { get; set; }

        public required string QuizTitle { get; set; }
        public required string Difficulty { get; set; }
        public required string TimeLimit { get; set; }

        public List<QuizAttemptQuestionDto> Questions { get; set; } = new();
    }
}