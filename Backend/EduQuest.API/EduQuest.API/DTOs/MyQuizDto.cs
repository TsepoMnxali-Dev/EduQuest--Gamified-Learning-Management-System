namespace EduQuest.API.DTOs.Learners
{
    /// <summary>One quiz as shown on the learner's Quizzes page.</summary>
    public class MyQuizDto
    {
        public int QuizID { get; set; }
        public required string QuizTitle { get; set; }
        public required string Difficulty { get; set; }

        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }
        public required string TopicName { get; set; }

        /// <summary>Every attempt serves exactly this many questions.</summary>
        public int QuestionCount { get; set; }

        /// <summary>True once the learner has submitted at least one attempt.</summary>
        public bool Completed { get; set; }
        public int AttemptCount { get; set; }

        // Details of the most recent submitted attempt (null if not completed).
        public int? LatestAttemptID { get; set; }
        public int? LatestPercentage { get; set; }
        public DateTime? LatestDateTaken { get; set; }
    }
}
