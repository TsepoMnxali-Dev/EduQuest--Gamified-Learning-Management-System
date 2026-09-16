using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateQuizOptionDto
    {
        [Required]
        [StringLength(300)]
        public required string OptionText { get; set; }

        public bool IsCorrect { get; set; }
    }
}