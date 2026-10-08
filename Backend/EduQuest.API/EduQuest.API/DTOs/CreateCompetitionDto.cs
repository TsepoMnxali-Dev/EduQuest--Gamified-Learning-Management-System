using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateCompetitionDto
    {
        [StringLength(100)]
        public string? SponsorName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Description { get; set; }

        [Required]
        public int SponsorID { get; set; }
    }
}