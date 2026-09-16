using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        [StringLength(150)]
        public required string Title { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        [Required]
        [StringLength(50)]
        public required string DateSent { get; set; }

        [Required]
        public int LearnerID { get; set; }
    }
}