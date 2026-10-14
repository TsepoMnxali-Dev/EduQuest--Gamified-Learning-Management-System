namespace EduQuest.API.DTOs
{
    public class LearnerClassDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TopicCount { get; set; }
        public int QuizzesCompleted { get; set; }
        public int QuizzesTotal { get; set; }
        public int ResourceCount { get; set; }
        public int ProgressPercent { get; set; }
        public int? AverageScorePercent { get; set; }
    }

    public class UpNextQuizDto
    {
        public int QuizID { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }

    public class LearnerClassesResponseDto
    {
        public string GradeName { get; set; } = string.Empty;
        public List<LearnerClassDto> Subjects { get; set; } = new();
        public UpNextQuizDto? UpNext { get; set; }
    }

    public class ClassMaterialDto
    {
        public int StudyMaterialID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public string? FileURL { get; set; }
        public bool HasFile { get; set; }
    }

    // One published quiz as the learner sees it.
    public class ClassQuizDto
    {
        public int QuizID { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }

        public int TopicID { get; set; }
        public string TopicName { get; set; } = string.Empty;

        // Approved questions the quiz has, and whether that is enough to start it.
        public int QuestionCount { get; set; }
        public bool IsReady { get; set; }

        // The learner's most recent submitted attempt, if any.
        public bool IsCompleted { get; set; }
        public int? LatestAttemptID { get; set; }
        public int? LatestPercentage { get; set; }
        public DateTime? LatestDate { get; set; }
    }

    public class ClassTopicDto
    {
        public int TopicID { get; set; }
        public string TopicName { get; set; } = string.Empty;

        // "Completed", "In Progress", "Not Started", "Coming Soon" or "No Quiz Yet"
        public string Status { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int QuizzesTotal { get; set; }

        public List<ClassQuizDto> Quizzes { get; set; } = new();
        public List<ClassMaterialDto> Materials { get; set; } = new();
    }

    public class LearnerClassDetailDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int QuizzesTotal { get; set; }
        public int ProgressPercent { get; set; }
        public int? AverageScorePercent { get; set; }

        // Every published quiz for this subject and grade, whatever its topic.
        public List<ClassQuizDto> Quizzes { get; set; } = new();
        public List<ClassTopicDto> Topics { get; set; } = new();
    }
}