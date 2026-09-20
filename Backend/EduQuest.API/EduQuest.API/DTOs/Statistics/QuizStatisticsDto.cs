namespace EduQuest.API.DTOs.Statistics
{
    public class QuizStatisticsDto
    {
        public int TotalAttempts { get; set; }
        public int UniqueLearners { get; set; }
    }

    public class QuizCompletionStatisticsDto
    {
        public int TotalCompleted { get; set; }
        public int UniqueLearners { get; set; }
    }
}