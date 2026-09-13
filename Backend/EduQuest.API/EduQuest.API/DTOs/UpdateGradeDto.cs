using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{

    public class UpdateGradeDto
    {
        [Required]
        [StringLength(50)]
        public required string GradeName { get; set; }
    }
}