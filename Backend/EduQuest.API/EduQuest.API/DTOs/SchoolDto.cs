namespace EduQuest.API.DTOs.Schools
{
    public class SchoolDto
    {
        public int SchoolID { get; set; }

        public required string SchoolName { get; set; }

        public int ProvinceID { get; set; }
        public required string ProvinceName { get; set; }
    }
}