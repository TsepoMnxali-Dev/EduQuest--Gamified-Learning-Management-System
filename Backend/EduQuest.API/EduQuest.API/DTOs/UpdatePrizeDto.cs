using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class UpdatePrizeDto
    {
        [Required]
        [StringLength(100)]
        public required string PrizeName { get; set; }

        [Required]
        [StringLength(500)]
        public required string PrizeDescription { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative.")]
        public double Value { get; set; }

        [Required]
        public int CompetitionID { get; set; }
    }
}