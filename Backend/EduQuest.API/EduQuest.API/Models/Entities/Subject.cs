namespace EduQuest.API.Models.Entities
{
    public class Subject
    {
        public int SubjectID { get; set; }

        public required string SubjectName { get; set; }
        public int GradeLevel { get; set; }



        public ICollection<Topic>? Topics { get; set; }

    }
}
