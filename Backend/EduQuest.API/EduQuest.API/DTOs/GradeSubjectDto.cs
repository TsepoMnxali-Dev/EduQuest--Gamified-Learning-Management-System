namespace EduQuest.API.DTOs
{
    public class GradeSubjectDto
    {
        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }
        public required string Description { get; set; }
    }
}
