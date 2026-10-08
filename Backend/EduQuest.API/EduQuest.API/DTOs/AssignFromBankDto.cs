using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class AssignFromBankDto
    {
        [Required]
        [Range(1, 100)]
        public int Count { get; set; }
    }
}