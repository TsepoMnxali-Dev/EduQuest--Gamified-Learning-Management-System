using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Schools
{
    public class CreateSchoolDto
    {
        [Required]
        [StringLength(200)]
        public required string SchoolName { get; set; }

        [Required]
        public int ProvinceID { get; set; }
    }
}