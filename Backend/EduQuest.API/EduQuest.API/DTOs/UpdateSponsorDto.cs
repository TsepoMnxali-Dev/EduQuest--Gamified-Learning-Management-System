using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class UpdateSponsorDto
    {
        [Required]
        [StringLength(100)]
        public required string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public required string ContactEmail { get; set; }
    }
}