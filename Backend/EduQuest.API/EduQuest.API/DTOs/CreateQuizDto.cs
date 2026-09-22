using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EduQuest.API.DTOs
{
    public class CreateQuizDto
    {
        [Required]
        [StringLength(100)]
        public required string QuizTitle { get; set; }

        [Required]
        [StringLength(20)]
        public required string Difficulty { get; set; }

        [Required]
        //public required string TimeLimit { get; set; }
        public TimeSpan TimeLimit { get; set; }

        public bool IsPublished { get; set; } = false;

        [Required]
        public int TopicID { get; set; }
    }
}