namespace EduQuest.API.DTOs
{
    public class CreateStudyMaterialDto
    {
        public int SubjectID { get; set; }
        public int GradeID { get; set; }
        public string Title { get; set; }
        public string FileURL { get; set; }
        public string ResourceType { get; set; }
    }
}
