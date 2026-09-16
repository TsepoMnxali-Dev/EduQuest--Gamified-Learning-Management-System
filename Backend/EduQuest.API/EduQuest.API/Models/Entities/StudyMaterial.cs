namespace EduQuest.API.Models.Entities
{
    public class StudyMaterial
    {
        public int StudyMaterialID { get; set; }

        public int TopicID { get; set; }
        public Topic? Topic { get; set; } = null;

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required string ResourceType { get; set; }

        public byte[]? FileData { get; set; }

        public string? FileName { get; set; }

        public string? FileContentType { get; set; }

        public string? FileURL { get; set; }
    }
}