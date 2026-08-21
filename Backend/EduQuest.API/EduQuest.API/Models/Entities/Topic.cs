namespace EduQuest.API.Models.Entities
{
    public class Topic
    {
        public int TopicID { get; set; }
        public int SubjectID { get; set; }
        public string GradeLevel { get; set; }
        public Subject Subject { get; set; }
    }
}
