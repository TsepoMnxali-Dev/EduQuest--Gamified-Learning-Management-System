namespace EduQuest.API.DTOs
{
    public class CreateSubjectDto
    {
        public required string SubjectName { get; set; }
        public required string GradeLevel { get; set; }
    }
}
