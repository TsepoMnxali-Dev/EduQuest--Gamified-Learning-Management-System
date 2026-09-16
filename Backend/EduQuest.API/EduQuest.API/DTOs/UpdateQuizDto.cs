using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class UpdateQuizDto
    {
        [Required]
        [StringLength(100)]
        public required string QuizTitle { get; set; }

        [Required]
        [StringLength(20)]
        public required string Difficulty { get; set; }

        [Required]
        [StringLength(20)]
        public required string TimeLimit { get; set; }

        public bool IsPublished { get; set; }

        [Required]
        public int TopicID { get; set; }
    }
}