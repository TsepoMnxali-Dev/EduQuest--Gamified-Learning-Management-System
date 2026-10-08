using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateQuizQuestionDto
    {
        [Required]
        [StringLength(500)]
        public required string QuestionText { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Explanation { get; set; }

        public bool GeneratedByAI { get; set; }
    
        public bool ApprovedByAdmin { get; set; }
    }
}