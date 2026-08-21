namespace EduQuest.API.DTOs.Learners
{
    public class UpdateLearnerDto
    {
        public int GradeID { get; set; }

        public string SchoolName { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;
    }
}
