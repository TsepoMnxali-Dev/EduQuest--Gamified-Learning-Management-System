using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateQuizAttemptDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Score cannot be negative.")]
        public int Score { get; set; }

        [Required]
        [StringLength(50)]
        public required string DateTaken { get; set; }

        [Required]
        [StringLength(50)]
        public required string TimeTaken { get; set; }

        [Required]
        public int QuizID { get; set; }

        [Required]
        public int LearnerID { get; set; }
    }
}