namespace EduQuest.API.DTOs.Statistics
{
    public class LearnerCountByProvinceDto
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public int LearnerCount { get; set; }
    }

    public class LearnerCountBySchoolDto
    {
        public int SchoolID { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public int LearnerCount { get; set; }
    }

    public class LearnerCountByGradeDto
    {
        public int GradeID { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public int LearnerCount { get; set; }
    }
}