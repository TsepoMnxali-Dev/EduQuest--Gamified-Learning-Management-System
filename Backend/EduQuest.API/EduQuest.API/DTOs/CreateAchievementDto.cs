using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateAchievementDto
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "PointsRequired cannot be negative.")]
        public int PointsRequired { get; set; }

        [Required]
        [StringLength(300)]
        public required string BadgeImage { get; set; }
    }
}