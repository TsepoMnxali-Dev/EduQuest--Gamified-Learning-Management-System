using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateCompetitionDto
    {
        [StringLength(100)]
        public string? SponsorName { get; set; }

        [Required]
        [StringLength(50)]
        public required string StartDate { get; set; }

        [Required]
        [StringLength(50)]
        public required string EndDate { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Description { get; set; }

        [Required]
        public int SponsorID { get; set; }
    }
}