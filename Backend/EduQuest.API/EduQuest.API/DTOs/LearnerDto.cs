namespace EduQuest.API.DTOs.Learners
{
    public class LearnerDto
    {
        public int LearnerID { get; set; }
        public int UserID { get; set; }
        public int GradeID { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
    }
}