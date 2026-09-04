namespace EduQuest.API.DTOs
{
    public class TopicDto
    {
        public int TopicID { get; set; }
        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }
        public required string TopicName { get; set; }
        public required string GradeLevel { get; set; }
    }
}