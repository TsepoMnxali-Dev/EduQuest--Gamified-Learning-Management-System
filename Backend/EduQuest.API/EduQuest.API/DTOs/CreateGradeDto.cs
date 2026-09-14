using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateGradeDto
    {
        [Required]
        [StringLength(50)]
        public required string GradeName { get; set; }
    }
}
