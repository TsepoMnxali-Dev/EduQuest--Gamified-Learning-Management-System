namespace EduQuest.API.DTOs
{
    public class SubjectDto
    {
        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }
        public required string GradeLevel { get; set; }
    }
}
