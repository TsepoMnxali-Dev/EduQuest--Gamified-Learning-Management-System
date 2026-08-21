namespace EduQuest.API.DTOs.Learners
{
    public class CreateLearnerDto
    {
        public int UserID { get; set; }

        public int GradeID { get; set; }

        public string SchoolName { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;
    }
}
