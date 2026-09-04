namespace EduQuest.API.DTOs
{
    public class StudyMaterialDto
    {
        public int StudyMaterialID { get; set; }
        public int SubjectID { get; set; }
        public int GradeID { get; set; }

        public required string SubjectName { get; set; }
        public required string GradeName { get; set; }

        public required string Title { get; set; }
        public required string FileURL { get; set; }
        public required string ResourceType { get; set; }
    }
}