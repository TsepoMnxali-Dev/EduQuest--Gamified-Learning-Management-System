namespace EduQuest.API.Models.Entities
{
    public class Subject
    {
        public int SubjectID {  get; set; }
        public string SubjectName { get; set; }
        public string GradeLevel { get; set; }
        public ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();
    }
}
