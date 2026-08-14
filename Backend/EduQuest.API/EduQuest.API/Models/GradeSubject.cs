namespace EduQuest.API.Models
{
    public class GradeSubject
    {
        public int GradeID { get; set; }
        public int SubjectID { get; set; }
        public string Description { get; set; }
        public Grade Grade { get; set; }
        public Subject Subject { get; set; }
    }
}
