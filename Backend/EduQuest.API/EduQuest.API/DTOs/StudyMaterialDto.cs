namespace EduQuest.API.DTOs
{
    public class StudyMaterialDto
    {
        public int StudyMaterialID { get; set; }

        public int TopicID { get; set; }
        public required string TopicName { get; set; }

        public int SubjectID { get; set; }
        public required string SubjectName { get; set; }

        public required string GradeLevel { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }

        public required string ResourceType { get; set; }

        public string? FileName { get; set; }
        public string? FileURL { get; set; }
    }
}