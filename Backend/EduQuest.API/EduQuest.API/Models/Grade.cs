namespace EduQuest.API.Models
{
    public class Grade
    {
        public int GradeID { get; set; }
        public string GradeName { get; set; }
        public ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();
    }
}
