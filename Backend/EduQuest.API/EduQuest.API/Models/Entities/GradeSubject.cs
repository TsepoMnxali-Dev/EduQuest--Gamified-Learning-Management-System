namespace EduQuest.API.Models.Entities
{
    public class GradeSubject
    {
        public int GradeSubjectID { get; set; }

        public int GradeID { get; set; }
        public Grade Grade { get; set; }

        public int SubjectID { get; set; }
        public Subject Subject { get; set; }

        public required string Description { get; set; }
    }
}
