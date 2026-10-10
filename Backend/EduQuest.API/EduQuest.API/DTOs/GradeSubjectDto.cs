namespace EduQuest.API.DTOs
{
    public class GradeSubjectDto
    {
        // The admin "Add Resource" form needs this ID, because a study
        // material belongs to a GradeSubject (a subject within a grade).
        public int GradeSubjectID { get; set; }
        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }
    }
}