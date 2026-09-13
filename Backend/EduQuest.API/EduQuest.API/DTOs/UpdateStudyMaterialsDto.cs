namespace EduQuest.API.DTOs
{
    public class UpdateStudyMaterialDto
    {
        public int SubjectID { get; set; }
        public int GradeID { get; set; }

        public required string Title { get; set; }
        public required string FileURL { get; set; }
        public required string ResourceType { get; set; }
    }
}