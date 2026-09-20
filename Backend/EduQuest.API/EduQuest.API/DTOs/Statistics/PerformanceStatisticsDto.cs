namespace EduQuest.API.DTOs.Statistics
{
    public class AverageScoreByProvinceDto
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }

    public class AverageScoreBySchoolDto
    {
        public int SchoolID { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }

    public class AverageScoreByGradeDto
    {
        public int GradeID { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }

    public class AverageScoreBySubjectDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }

    public class AverageScoreByTopicDto
    {
        public int TopicID { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
    }
    public class PassFailStatisticsDto
    {
        public int TotalAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public double PassRate { get; set; }
        public double FailRate { get; set; }
    }
}