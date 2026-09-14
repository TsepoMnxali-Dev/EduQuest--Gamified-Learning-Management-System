namespace EduQuest.API.DTOs.Learners
{
    public class LearnerDto
    {
        public int LearnerID { get; set; }
        public int UserID { get; set; }

        public int GradeID { get; set; }
        public required string GradeName { get; set; }

        public int SchoolID { get; set; }
        public required string SchoolName { get; set; }

        public int ProvinceID { get; set; }
        public required string ProvinceName { get; set; }
    }
}