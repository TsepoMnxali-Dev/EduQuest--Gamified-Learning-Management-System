using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Users
{
    public class UpdateUserDto
    {
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}