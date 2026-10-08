using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateQuestionBankItemDto
    {
        [Required]
        public int TopicID { get; set; }

        [Required]
        [StringLength(500)]
        public required string QuestionText { get; set; }

        [StringLength(2000)]
        public string? SourceExtract { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Explanation { get; set; }

        [Required]
        [StringLength(20)]
        public required string Difficulty { get; set; }

        public List<CreateQuestionBankOptionDto> Options { get; set; } = new();
    }
}