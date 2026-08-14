namespace EduQuest.API.DTOs
{
    public class StudyMaterialDto
    {
        public int StudyMaterialID { get; set; }
        public int SubjectID { get; set; }
        public int GradeID { get; set; }
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public string Title { get; set; }
        public string FileURL { get; set; }
        public string ResourceType { get; set; }

    }
}
