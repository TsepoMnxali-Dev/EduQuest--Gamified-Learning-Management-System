namespace EduQuest.API.Models
{
    public class StudyMaterial
    {
        public int StudyMaterialID { get; set; }
        public int SubjectID { get; set; }
        public int GradeID { get; set; }
        public string Title { get; set; }
        public string FileURL { get; set; }
        public string ResourceType { get; set; }
        public Subject Subject { get; set; }
        public Grade Grade { get; set; }
    }
}
