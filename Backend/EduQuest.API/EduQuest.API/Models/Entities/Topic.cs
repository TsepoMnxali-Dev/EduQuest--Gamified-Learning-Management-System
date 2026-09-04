namespace EduQuest.API.Models.Entities
{
    public class Topic
    {
        public int TopicID { get; set; }

        public int SubjectID { get; set; }
        public required Subject Subject { get; set; }

        public required string TopicName { get; set; }
        public required string GradeLevel { get; set; }

        public ICollection<Quiz> Quiz { get; set; } = new List<Quiz>();
    }
}