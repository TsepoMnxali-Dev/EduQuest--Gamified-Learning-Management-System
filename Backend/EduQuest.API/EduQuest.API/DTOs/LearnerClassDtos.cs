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

    public class ClassTopicDto
    {
        public int TopicID { get; set; }
        public string TopicName { get; set; } = string.Empty;

        // "Completed", "In Progress", "Not Started" or "No Quiz Yet"
        public string Status { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int QuizzesTotal { get; set; }

        // The quiz the learner should take (or retake) for this topic.
        public int? NextQuizID { get; set; }
        public string? NextQuizTitle { get; set; }
        public bool NextQuizIsRetake { get; set; }

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
        public List<ClassTopicDto> Topics { get; set; } = new();
    }
}