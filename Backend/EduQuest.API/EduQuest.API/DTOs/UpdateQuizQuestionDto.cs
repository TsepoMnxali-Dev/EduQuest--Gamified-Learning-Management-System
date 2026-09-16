using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class UpdateQuizQuestionDto
    {
        [Required]
        [StringLength(500)]
        public required string QuestionText { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Explanation { get; set; }

        [Required]
        [StringLength(10)]
        public required string GeneratedByAI { get; set; }

        [Required]
        [StringLength(10)]
        public required string ApprovedByAdmin { get; set; }
    }
}