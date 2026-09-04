namespace EduQuest.API.DTOs
{
    public class CreateTopicDto
    {
        public int SubjectID { get; set; }
        public required string TopicName { get; set; }
        public required string GradeLevel { get; set; }
    }
}