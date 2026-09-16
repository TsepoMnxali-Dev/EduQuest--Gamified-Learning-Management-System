using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduQuest.API.DTOs
{
    public class CreateStudyMaterialDto
    {
        [Range(1, int.MaxValue)]
        public int TopicID { get; set; }

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public required string ResourceType { get; set; }

        public IFormFile? File { get; set; }

        [Url]
        [StringLength(500)]
        public string? FileURL { get; set; }
    }
}